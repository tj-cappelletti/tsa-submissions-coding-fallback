using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using TsaSubmissions.Web.Configuration;
using TsaSubmissions.Web.Data;
using TsaSubmissions.Web.Models;
using TsaSubmissions.Web.ViewModels;

namespace TsaSubmissions.Web.Controllers;

[Authorize(Roles = nameof(AppRole.Judge))]
public class JudgeController(ApplicationDbContext dbContext, IOptions<EventSettings> eventSettings) : Controller
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();
    private readonly EventSettings _eventSettings = eventSettings.Value;
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

        string displayCode;
        try
        {
            displayCode = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
                .GetString(submission.FileContent);
        }
        catch (DecoderFallbackException)
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

    [HttpGet]
    public IActionResult BulkImportUsers()
    {
        return View(new BulkImportUsersViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkImportUsers(BulkImportUsersViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = new BulkImportResultViewModel();

        using var reader = new StreamReader(model.CsvFile!.OpenReadStream());

        // Read header
        var header = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(header))
        {
            ModelState.AddModelError(nameof(model.CsvFile), "CSV file is empty.");
            return View(model);
        }

        var headers = header.Split(',').Select(h => h.Trim().ToLowerInvariant()).ToList();
        var usernameIndex = headers.IndexOf("username");
        var passwordIndex = headers.IndexOf("password");

        if (usernameIndex == -1 || passwordIndex == -1)
        {
            ModelState.AddModelError(nameof(model.CsvFile), "CSV file must contain 'username' and 'password' columns.");
            return View(model);
        }

        var lineNumber = 1;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = line.Split(',').Select(v => v.Trim()).ToList();

            if (values.Count <= Math.Max(usernameIndex, passwordIndex))
            {
                result.Errors.Add($"Line {lineNumber}: Invalid format - not enough columns.");
                result.FailureCount++;
                continue;
            }

            var username = values[usernameIndex];
            var password = values[passwordIndex];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                result.Errors.Add($"Line {lineNumber}: Username and password cannot be empty.");
                result.FailureCount++;
                continue;
            }

            // Check if user already exists
            var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existingUser != null)
            {
                result.Errors.Add($"Line {lineNumber}: User '{username}' already exists.");
                result.FailureCount++;
                continue;
            }

            // Create new user
            var newUser = new AppUser
            {
                Username = username,
                Role = AppRole.Participant
            };
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);

            try
            {
                dbContext.Users.Add(newUser);
                await dbContext.SaveChangesAsync();
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Line {lineNumber}: Error creating user '{username}' - {ex.Message}");
                result.FailureCount++;
            }
        }

        return View("BulkImportResult", result);
    }

    [HttpGet]
    public async Task<IActionResult> UserList()
    {
        var users = await dbContext.Users
            .Where(u => u.Role == AppRole.Participant)
            .Include(u => u.Sessions)
            .OrderBy(u => u.Username)
            .ToListAsync();

        var estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        var userViewModels = users.Select(u =>
        {
            var session = u.Sessions.FirstOrDefault();
            DateTime? endTimeUtc = null;
            DateTime? startedAtEst = null;
            DateTime? endTimeEst = null;

            if (session != null)
            {
                endTimeUtc = session.StartedAtUtc.AddHours(_eventSettings.ParticipantDurationHours);
                startedAtEst = TimeZoneInfo.ConvertTimeFromUtc(session.StartedAtUtc, estTimeZone);
                endTimeEst = TimeZoneInfo.ConvertTimeFromUtc(endTimeUtc.Value, estTimeZone);
            }

            return new UserListViewModel
            {
                Username = u.Username,
                StartedAtUtc = session?.StartedAtUtc,
                EndTimeUtc = endTimeUtc,
                StartedAtEst = startedAtEst,
                EndTimeEst = endTimeEst
            };
        }).ToList();

        return View(userViewModels);
    }

    // CRUD Operations for Problems

    [HttpGet]
    public IActionResult CreateProblem()
    {
        return View(new CreateProblemViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProblem(CreateProblemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var problem = new Problem
        {
            Title = model.Title,
            DescriptionMarkdown = model.DescriptionMarkdown
        };

        dbContext.Problems.Add(problem);
        await dbContext.SaveChangesAsync();

        // Add starter codes
        await AddOrUpdateStarterCodes(problem.Id, model);

        TempData["Success"] = $"Problem '{problem.Title}' created successfully.";
        return RedirectToAction("Index", "Problems");
    }

    [HttpGet]
    public async Task<IActionResult> EditProblem(int id)
    {
        var problem = await dbContext.Problems
            .Include(p => p.StarterCodes)
            .SingleOrDefaultAsync(p => p.Id == id);

        if (problem is null)
        {
            return NotFound();
        }

        var vm = new EditProblemViewModel
        {
            Id = problem.Id,
            Title = problem.Title,
            DescriptionMarkdown = problem.DescriptionMarkdown,
            CppStarterCode = problem.StarterCodes.FirstOrDefault(s => s.Language == SupportedLanguage.Cpp)?.Code,
            CSharpStarterCode = problem.StarterCodes.FirstOrDefault(s => s.Language == SupportedLanguage.CSharp)?.Code,
            JavaStarterCode = problem.StarterCodes.FirstOrDefault(s => s.Language == SupportedLanguage.Java)?.Code,
            JavaScriptStarterCode = problem.StarterCodes.FirstOrDefault(s => s.Language == SupportedLanguage.JavaScript)?.Code,
            PythonStarterCode = problem.StarterCodes.FirstOrDefault(s => s.Language == SupportedLanguage.Python)?.Code
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProblem(EditProblemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var problem = await dbContext.Problems
            .Include(p => p.StarterCodes)
            .SingleOrDefaultAsync(p => p.Id == model.Id);

        if (problem is null)
        {
            return NotFound();
        }

        problem.Title = model.Title;
        problem.DescriptionMarkdown = model.DescriptionMarkdown;

        await dbContext.SaveChangesAsync();

        // Update starter codes
        await AddOrUpdateStarterCodes(problem.Id, model);

        TempData["Success"] = $"Problem '{problem.Title}' updated successfully.";
        return RedirectToAction("Index", "Problems");
    }

    [HttpGet]
    public async Task<IActionResult> DeleteProblem(int id)
    {
        var problem = await dbContext.Problems
            .Include(p => p.Submissions)
            .SingleOrDefaultAsync(p => p.Id == id);

        if (problem is null)
        {
            return NotFound();
        }

        return View(problem);
    }

    [HttpPost, ActionName("DeleteProblem")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProblemConfirmed(int id)
    {
        var problem = await dbContext.Problems
            .Include(p => p.StarterCodes)
            .Include(p => p.Submissions)
            .SingleOrDefaultAsync(p => p.Id == id);

        if (problem is null)
        {
            return NotFound();
        }

        // Remove starter codes
        dbContext.ProblemStarterCodes.RemoveRange(problem.StarterCodes);

        // Remove submissions
        dbContext.Submissions.RemoveRange(problem.Submissions);

        // Remove problem
        dbContext.Problems.Remove(problem);

        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"Problem '{problem.Title}' deleted successfully.";
        return RedirectToAction("Index", "Problems");
    }

    private async Task AddOrUpdateStarterCodes(int problemId, CreateProblemViewModel model)
    {
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.Cpp, model.CppStarterCode);
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.CSharp, model.CSharpStarterCode);
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.Java, model.JavaStarterCode);
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.JavaScript, model.JavaScriptStarterCode);
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.Python, model.PythonStarterCode);
    }

    private async Task AddOrUpdateStarterCodes(int problemId, EditProblemViewModel model)
    {
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.Cpp, model.CppStarterCode);
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.CSharp, model.CSharpStarterCode);
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.Java, model.JavaStarterCode);
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.JavaScript, model.JavaScriptStarterCode);
        await AddOrUpdateStarterCode(problemId, SupportedLanguage.Python, model.PythonStarterCode);
    }

    private async Task AddOrUpdateStarterCode(int problemId, SupportedLanguage language, string? code)
    {
        var existingStarterCode = await dbContext.ProblemStarterCodes
            .FirstOrDefaultAsync(s => s.ProblemId == problemId && s.Language == language);

        if (string.IsNullOrWhiteSpace(code))
        {
            // Remove if exists and code is empty
            if (existingStarterCode != null)
            {
                dbContext.ProblemStarterCodes.Remove(existingStarterCode);
                await dbContext.SaveChangesAsync();
            }
        }
        else
        {
            if (existingStarterCode != null)
            {
                // Update existing
                existingStarterCode.Code = code;
            }
            else
            {
                // Add new
                var newStarterCode = new ProblemStarterCode
                {
                    ProblemId = problemId,
                    Language = language,
                    Code = code
                };
                dbContext.ProblemStarterCodes.Add(newStarterCode);
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
