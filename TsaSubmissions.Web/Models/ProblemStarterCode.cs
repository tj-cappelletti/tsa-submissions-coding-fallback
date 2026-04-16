using System.ComponentModel.DataAnnotations;

namespace TsaSubmissions.Web.Models;

public class ProblemStarterCode
{
    public int Id { get; set; }

    public int ProblemId { get; set; }
    public Problem Problem { get; set; } = null!;

    [Required]
    public SupportedLanguage Language { get; set; }

    [Required]
    public string Code { get; set; } = string.Empty;
}
