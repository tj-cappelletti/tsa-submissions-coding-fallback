using TsaSubmissions.Web.Models;

namespace TsaSubmissions.Web.ViewModels;

public class ProblemDetailsViewModel
{
    public Problem Problem { get; set; } = null!;
    public string DescriptionHtml { get; set; } = string.Empty;
    public SupportedLanguage SelectedLanguage { get; set; }
    public string StarterCode { get; set; } = string.Empty;
    public DateTime? EventStartedAtUtc { get; set; }
}
