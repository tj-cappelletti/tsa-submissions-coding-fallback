namespace TsaSubmissions.Web.ViewModels;

public class UserListViewModel
{
    public string Username { get; set; } = string.Empty;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; }
    public DateTime? StartedAtEst { get; set; }
    public DateTime? EndTimeEst { get; set; }
}
