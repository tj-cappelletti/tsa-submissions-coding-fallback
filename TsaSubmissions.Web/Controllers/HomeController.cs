using Microsoft.AspNetCore.Mvc;

namespace TsaSubmissions.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Problems");
        }

        return RedirectToAction("Login", "Account");
    }
}
