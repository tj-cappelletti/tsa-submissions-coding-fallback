using System.ComponentModel.DataAnnotations;

namespace TsaSubmissions.Web.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public AppRole Role { get; set; }

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<ParticipantSession> Sessions { get; set; } = new List<ParticipantSession>();
}
