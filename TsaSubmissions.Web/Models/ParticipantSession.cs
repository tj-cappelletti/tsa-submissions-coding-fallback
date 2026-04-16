namespace TsaSubmissions.Web.Models;

public class ParticipantSession
{
    public int Id { get; set; }

    public int ParticipantId { get; set; }
    public AppUser Participant { get; set; } = null!;

    public DateTime StartedAtUtc { get; set; }
}
