namespace TsaSubmissions.Web.Configuration;

public class EventSettings
{
    public DateTime EventEndTimeUtc { get; set; }
    public int ParticipantDurationHours { get; set; } = 2;
    public int TotalEventWindowHours { get; set; } = 3;
}
