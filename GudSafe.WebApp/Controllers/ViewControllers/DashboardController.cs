using GudSafe.Data;
using GudSafe.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GudSafe.WebApp.Controllers;

public class DashboardController : Controller
{
    // GET
    public IActionResult Index()
    {
        using var context = new GudSafeContext();

        var files = context.Users.FirstOrDefault()?.FilesUploaded ?? new ();

        return View(new DashboardViewModel
        {
            Files = files.ToList()
        });
    }
}