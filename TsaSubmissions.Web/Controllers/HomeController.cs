using Markdig;
using Microsoft.AspNetCore.Mvc;

namespace TsaSubmissions.Web.Controllers;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build();

    public HomeController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Problems");
        }

        return RedirectToAction("Login", "Account");
    }

    public IActionResult Scoring()
    {
        var scoringFilePath = Path.Combine(_environment.ContentRootPath, "scoring.md");

        if (!System.IO.File.Exists(scoringFilePath))
        {
            return NotFound("Scoring documentation not found.");
        }

        var markdownContent = System.IO.File.ReadAllText(scoringFilePath);
        var htmlContent = Markdown.ToHtml(markdownContent, _markdownPipeline);

        ViewBag.ScoringHtml = htmlContent;
        return View();
    }
}
