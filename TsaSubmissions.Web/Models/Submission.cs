using System.ComponentModel.DataAnnotations;

namespace TsaSubmissions.Web.Models;

public class Submission
{
    public int Id { get; set; }

    public int ProblemId { get; set; }
    public Problem Problem { get; set; } = null!;

    public int ParticipantId { get; set; }
    public AppUser Participant { get; set; } = null!;

    [Required]
    public SupportedLanguage Language { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string ContentType { get; set; } = "application/octet-stream";

    [Required]
    public byte[] FileContent { get; set; } = [];

    public DateTime UploadedAtUtc { get; set; }
}
