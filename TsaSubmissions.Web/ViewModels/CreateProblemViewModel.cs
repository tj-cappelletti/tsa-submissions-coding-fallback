using System.ComponentModel.DataAnnotations;
using TsaSubmissions.Web.Models;

namespace TsaSubmissions.Web.ViewModels;

public class CreateProblemViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Problem Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [Display(Name = "Description (Markdown)")]
    public string DescriptionMarkdown { get; set; } = string.Empty;

    [Display(Name = "C++ Starter Code")]
    public string? CppStarterCode { get; set; }

    [Display(Name = "C# Starter Code")]
    public string? CSharpStarterCode { get; set; }

    [Display(Name = "Java Starter Code")]
    public string? JavaStarterCode { get; set; }

    [Display(Name = "JavaScript Starter Code")]
    public string? JavaScriptStarterCode { get; set; }

    [Display(Name = "Python Starter Code")]
    public string? PythonStarterCode { get; set; }
}
