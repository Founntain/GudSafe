using System.ComponentModel;
using System.Net;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Models;
using GudSafe.Data.ViewModels;
using GudSafe.WebApp.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GudSafe.WebApp.Controllers;

public class DashboardController : Controller
{
    private readonly GudFileController _fileController;
    private readonly GudSafeContext _context;
    private readonly IMapper _mapper;

    public DashboardController(GudFileController fileController, GudSafeContext context, IMapper mapper)
    {
        _fileController = fileController;
        _context = context;
        _mapper = mapper;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Gallery()
    {
        var files = _context.Users.FirstOrDefault()?.FilesUploaded.OrderByDescending(x => x.CreationTime).ToList() ?? new ();

        return View(new GalleryViewModel
        {
            Files = files.ToList()
        });
    }

    public async Task<IActionResult> UserSettings()
    {
        var user = await _context.Users.FirstOrDefaultAsync();

        var viewmodel = new UserSettingsViewModel
        {
            User = _mapper.Map<UserModel>(user),
            ApiKey = user.ApiKey
        };

        return View(viewmodel);
    }

    public async Task<IActionResult> ShareXProfile()
    {
        var user = await _context.Users.FirstOrDefaultAsync();

        if (user == default) return NotFound("User not found");

        var shareXProfile = new ShareXProfile
        {
            Name = $"GudSafe {Request.Host}",
            DestinationType = "ImageUploader, FileUploader",
            RequestMethod = "POST",
            RequestURL = $"{Request.Scheme}://{Request.Host}/api/files/upload",
            Body = "MultipartFormData",
            Headers =
                new Dictionary<string, object>
                {
                    { "apikey", user.ApiKey }
                },
            FileFormName = "file",
            URL = "{json:url}",
            ThumbnailURL = "{json:thumbnailurl}"
        };

        var json = JsonConvert.SerializeObject(shareXProfile);

        var data = Encoding.UTF8.GetBytes(json);
        
        return File(data, "application/octet-stream", $"GudSafe-{Request.Host}.sxcu");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _fileController.Delete(id);

        var files = _context.Users.FirstOrDefault()?.FilesUploaded ?? new();

        return View("Index", new GalleryViewModel
        {
            Files = files.ToList()
        });
    }
}