using System.Security.Claims;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TsaSubmissions.Web.Configuration;
using TsaSubmissions.Web.Data;
using TsaSubmissions.Web.Models;
using TsaSubmissions.Web.ViewModels;

namespace TsaSubmissions.Web.Controllers;

[Authorize]
public class ProblemsController(ApplicationDbContext dbContext, IOptions<EventSettings> eventSettings) : Controller
{
    private const long MaxSubmissionBytes = 5 * 1024 * 1024;
    private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build();
    private readonly EventSettings _eventSettings = eventSettings.Value;

    public async Task<IActionResult> Index()
    {
        var problems = await dbContext.Problems.OrderBy(p => p.Title).ToListAsync();
        return View(problems);
    }

    [Authorize(Roles = nameof(AppRole.Participant))]
    public async Task<IActionResult> Details(int id, SupportedLanguage language = SupportedLanguage.CSharp)
    {
        var problem = await dbContext.Problems
            .Include(p => p.StarterCodes)
            .SingleOrDefaultAsync(p => p.Id == id);

        if (problem is null)
        {
            return NotFound();
        }

        var participantId = GetCurrentUserId();
        var session = await dbContext.ParticipantSessions.SingleOrDefaultAsync(s => s.ParticipantId == participantId);
        if (session is null)
        {
            session = new ParticipantSession
            {
                ParticipantId = participantId,
                StartedAtUtc = DateTime.UtcNow
            };

            dbContext.ParticipantSessions.Add(session);
            await dbContext.SaveChangesAsync();
        }

        var starterCode = problem.StarterCodes.SingleOrDefault(s => s.Language == language)?.Code
            ?? problem.StarterCodes.OrderBy(s => s.Language).FirstOrDefault()?.Code
            ?? string.Empty;

        var participantDeadline = session.StartedAtUtc.AddHours(_eventSettings.ParticipantDurationHours);
        var eventDeadline = _eventSettings.EventEndTimeUtc;
        var submissionDeadline = participantDeadline < eventDeadline ? participantDeadline : eventDeadline;

        var vm = new ProblemDetailsViewModel
        {
            Problem = problem,
            DescriptionHtml = Markdown.ToHtml(problem.DescriptionMarkdown, _markdownPipeline),
            SelectedLanguage = language,
            StarterCode = starterCode,
            EventStartedAtUtc = session.StartedAtUtc,
            SubmissionDeadlineUtc = submissionDeadline,
            ParticipantDeadlineUtc = participantDeadline,
            EventDeadlineUtc = eventDeadline
        };

        return View(vm);
    }

    [Authorize(Roles = nameof(AppRole.Participant))]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadSubmission(int problemId, SupportedLanguage language, IFormFile submissionFile)
    {
        var problem = await dbContext.Problems.AnyAsync(p => p.Id == problemId);
        if (!problem)
        {
            return NotFound();
        }

        // Check if submission deadline has passed
        var participantId = GetCurrentUserId();
        var session = await dbContext.ParticipantSessions.SingleOrDefaultAsync(s => s.ParticipantId == participantId);
        if (session is not null)
        {
            var participantDeadline = session.StartedAtUtc.AddHours(_eventSettings.ParticipantDurationHours);
            var eventDeadline = _eventSettings.EventEndTimeUtc;
            var submissionDeadline = participantDeadline < eventDeadline ? participantDeadline : eventDeadline;

            if (DateTime.UtcNow > submissionDeadline)
            {
                TempData["Error"] = "Submission deadline has passed. You can no longer submit solutions.";
                return RedirectToAction(nameof(Details), new { id = problemId, language });
            }
        }

        if (submissionFile.Length == 0)
        {
            TempData["Error"] = "Please choose a non-empty file.";
            return RedirectToAction(nameof(Details), new { id = problemId, language });
        }

        if (submissionFile.Length > MaxSubmissionBytes)
        {
            TempData["Error"] = "Submission exceeds the 5 MB limit.";
            return RedirectToAction(nameof(Details), new { id = problemId, language });
        }

        await using var memoryStream = new MemoryStream();
        await submissionFile.CopyToAsync(memoryStream);

        var submission = new Submission
        {
            ProblemId = problemId,
            ParticipantId = GetCurrentUserId(),
            Language = language,
            FileName = Path.GetFileName(submissionFile.FileName),
            ContentType = string.IsNullOrWhiteSpace(submissionFile.ContentType) ? "application/octet-stream" : submissionFile.ContentType,
            FileContent = memoryStream.ToArray(),
            UploadedAtUtc = DateTime.UtcNow
        };

        dbContext.Submissions.Add(submission);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Submission uploaded.";
        return RedirectToAction(nameof(Details), new { id = problemId, language });
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Authenticated user id claim is missing.");
        }

        return userId;
    }

    [Authorize(Roles = nameof(AppRole.Participant))]
    public async Task<IActionResult> MySubmissions(int problemId)
    {
        var problem = await dbContext.Problems.SingleOrDefaultAsync(p => p.Id == problemId);
        if (problem is null)
        {
            return NotFound();
        }

        var participantId = GetCurrentUserId();
        var submissions = await dbContext.Submissions
            .Where(s => s.ProblemId == problemId && s.ParticipantId == participantId)
            .OrderByDescending(s => s.UploadedAtUtc)
            .ThenByDescending(s => s.Id)
            .ToListAsync();

        var vm = new JudgeProblemSubmissionsViewModel
        {
            Problem = problem,
            LatestSubmissions = submissions
        };

        return View(vm);
    }

    [Authorize(Roles = nameof(AppRole.Participant))]
    public async Task<IActionResult> ViewSubmission(int submissionId)
    {
        var participantId = GetCurrentUserId();
        var submission = await dbContext.Submissions
            .Include(s => s.Participant)
            .Include(s => s.Problem)
            .SingleOrDefaultAsync(s => s.Id == submissionId && s.ParticipantId == participantId);

        if (submission is null)
        {
            return NotFound();
        }

        string displayCode;
        try
        {
            displayCode = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
                .GetString(submission.FileContent);
        }
        catch (System.Text.DecoderFallbackException)
        {
            displayCode = "// Unable to display this submission as UTF-8 text.";
        }

        var vm = new SubmissionCodeViewModel
        {
            Submission = submission,
            DisplayCode = displayCode,
            PrismLanguageClass = submission.Language switch
            {
                SupportedLanguage.Cpp => "language-cpp",
                SupportedLanguage.CSharp => "language-csharp",
                SupportedLanguage.Java => "language-java",
                SupportedLanguage.JavaScript => "language-javascript",
                SupportedLanguage.Python => "language-python",
                _ => "language-clike"
            }
        };

        return View(vm);
    }

    [Authorize(Roles = nameof(AppRole.Participant))]
    public async Task<IActionResult> DownloadSubmission(int submissionId)
    {
        var participantId = GetCurrentUserId();
        var submission = await dbContext.Submissions
            .SingleOrDefaultAsync(s => s.Id == submissionId && s.ParticipantId == participantId);

        if (submission is null)
        {
            return NotFound();
        }

        return File(submission.FileContent, submission.ContentType, submission.FileName);
    }
}
