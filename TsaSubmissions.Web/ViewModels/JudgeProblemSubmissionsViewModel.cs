using TsaSubmissions.Web.Models;

namespace TsaSubmissions.Web.ViewModels;

public class JudgeProblemSubmissionsViewModel
{
    public Problem Problem { get; set; } = null!;
    public IReadOnlyCollection<Submission> LatestSubmissions { get; set; } = [];
}
