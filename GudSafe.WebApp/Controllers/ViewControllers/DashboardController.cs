using System.Runtime.Loader;
using GudSafe.Data;
using GudSafe.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GudSafe.WebApp.Controllers;

public class DashboardController : Controller
{
    private readonly GudFileController _fileController;
    private readonly GudSafeContext _context;

    public DashboardController(GudFileController fileController, GudSafeContext context)
    {
        _fileController = fileController;
        _context = context;
    }
    
    // GET
    public IActionResult Index()
    { 
        var files = _context.Users.FirstOrDefault()?.FilesUploaded ?? new ();

        return View(new DashboardViewModel
        {
            Files = files.ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _fileController.Delete(id);

        var files = _context.Users.FirstOrDefault()?.FilesUploaded ?? new ();
        
        return View("Index", new DashboardViewModel
        {
            Files = files.ToList()
        });
    }
}