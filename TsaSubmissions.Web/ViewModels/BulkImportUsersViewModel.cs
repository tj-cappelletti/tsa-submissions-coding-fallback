using System.ComponentModel.DataAnnotations;

namespace TsaSubmissions.Web.ViewModels;

public class BulkImportUsersViewModel
{
    [Required(ErrorMessage = "Please select a CSV file to upload.")]
    public IFormFile? CsvFile { get; set; }
}
