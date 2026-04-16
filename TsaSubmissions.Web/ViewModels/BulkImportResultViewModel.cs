namespace TsaSubmissions.Web.ViewModels;

public class BulkImportResultViewModel
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
