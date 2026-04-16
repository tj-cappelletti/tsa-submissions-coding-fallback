using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TsaSubmissions.Web.Data;
using TsaSubmissions.Web.Models;
using TsaSubmissions.Web.ViewModels;

namespace TsaSubmissions.Web.Controllers;

[Authorize(Roles = nameof(AppRole.Judge))]
public class JudgeController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> ProblemSubmissions(int problemId)
    {
        var problem = await dbContext.Problems.SingleOrDefaultAsync(p => p.Id == problemId);
        if (problem is null)
        {
            return NotFound();
        }

        var latestSubmissionIds = await dbContext.Submissions
            .Where(s => s.ProblemId == problemId)
            .GroupBy(s => s.ParticipantId)
            .Select(g => g.OrderByDescending(x => x.UploadedAtUtc).ThenByDescending(x => x.Id).Select(x => x.Id).First())
            .ToListAsync();

        var latestSubmissions = await dbContext.Submissions
            .Include(s => s.Participant)
            .Where(s => latestSubmissionIds.Contains(s.Id))
            .OrderBy(s => s.Participant.Username)
            .ToListAsync();

        var vm = new JudgeProblemSubmissionsViewModel
        {
            Problem = problem,
            LatestSubmissions = latestSubmissions
        };

        return View(vm);
    }

    public async Task<IActionResult> Download(int submissionId)
    {
        var submission = await dbContext.Submissions
            .SingleOrDefaultAsync(s => s.Id == submissionId);

        if (submission is null)
        {
            return NotFound();
        }

        return File(submission.FileContent, submission.ContentType, submission.FileName);
    }

    public async Task<IActionResult> ViewSubmission(int submissionId)
    {
        var submission = await dbContext.Submissions
            .Include(s => s.Participant)
            .Include(s => s.Problem)
            .SingleOrDefaultAsync(s => s.Id == submissionId);

        if (submission is null)
        {
            return NotFound();
        }

        var vm = new SubmissionCodeViewModel
        {
            Submission = submission,
            DisplayCode = System.Text.Encoding.UTF8.GetString(submission.FileContent),
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
}
