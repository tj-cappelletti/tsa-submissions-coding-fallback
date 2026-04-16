using System.ComponentModel.DataAnnotations;

namespace TsaSubmissions.Web.Models;

public class Problem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string DescriptionMarkdown { get; set; } = string.Empty;

    public ICollection<ProblemStarterCode> StarterCodes { get; set; } = new List<ProblemStarterCode>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
