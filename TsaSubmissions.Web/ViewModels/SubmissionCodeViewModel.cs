using TsaSubmissions.Web.Models;

namespace TsaSubmissions.Web.ViewModels;

public class SubmissionCodeViewModel
{
    public Submission Submission { get; set; } = null!;
    public string DisplayCode { get; set; } = string.Empty;
    public string PrismLanguageClass { get; set; } = "language-clike";
}
