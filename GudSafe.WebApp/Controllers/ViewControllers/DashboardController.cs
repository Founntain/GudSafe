using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text;
using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Cryptography;
using GudSafe.Data.Entities;
using GudSafe.Data.Enums;
using GudSafe.Data.Models.EntityModels;
using GudSafe.Data.ViewModels;
using GudSafe.WebApp.Classes;
using GudSafe.WebApp.Controllers.EntityControllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GudSafe.WebApp.Controllers.ViewControllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly GudSafeContext _context;
    private readonly GudFileController _fileController;
    private readonly IMapper _mapper;
    private readonly ILogger<DashboardController> _logger;
    private readonly INotyfService _notyf;

    public DashboardController(GudFileController fileController, GudSafeContext context, IMapper mapper,
        ILogger<DashboardController> logger, INotyfService notyf)
    {
        _fileController = fileController;
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notyf = notyf;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Gallery()
    {
        var user = await FindUser();

        var files = user?.FilesUploaded.OrderByDescending(x => x.CreationTime).ToList() ?? new List<GudFile>();

        return View(new GalleryViewModel
        {
            Username = user?.Name,
            Files = files.ToList()
        });
    }

    public async Task<IActionResult> UserSettings()
    {
        var user = await FindUser();

        var viewmodel = new UserSettingsViewModel
        {
            User = _mapper.Map<UserModel>(user),
            ApiKey = user?.ApiKey ?? string.Empty
        };

        return View(viewmodel);
    }

    public async Task<IActionResult> ShareXProfile()
    {
        var user = await FindUser();

        if (user == default)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            return BadRequest("User not found");
        }

        var shareXProfile = new ShareXProfile
        {
            Name = $"GudSafe {Request.Host}",
            DestinationType = "ImageUploader, FileUploader",
            RequestMethod = "POST",
            RequestURL = $"{Request.Scheme}://{Request.Host}/files/upload",
            Body = "MultipartFormData",
            Headers =
                new Dictionary<string, object>
                {
                    {"apikey", user.ApiKey}
                },
            FileFormName = "file",
            URL = "$json:url$",
            ThumbnailURL = "$json:thumbnailurl$"
        };

        var json = JsonConvert.SerializeObject(shareXProfile);

        var data = Encoding.UTF8.GetBytes(json);

        return File(data, "application/octet-stream", $"GudSafe-{Request.Host}.sxcu");
    }

    [HttpPost]
    public async Task<JsonResult> Delete(Guid id)
    {
        var user = await FindUser();

        if (user == default)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            _notyf.Error("User not found");

            return Json(new {success = false});
        }

        await _fileController.Delete(id);

        _notyf.Success($"File {id} deleted successfully", 2);

        return Json(new {success = true});
    }

    private Task<User?> FindUser()
    {
        return _context.Users
            .FirstOrDefaultAsync(x => x.Name == User.FindFirstValue(ClaimTypes.Name));
    }

    public async Task<IActionResult> AdminSettings()
    {
        var users = await _context.Users.Where(x => x.ID != 1).Select(x => new SelectListItem
        {
            Text = x.Name,
            Value = x.UniqueId.ToString()
        }).ToListAsync();

        return View(new AdminSettingsViewModel
        {
            Users = users
        });
    }

    [HttpPost]
    public async Task<JsonResult> CreateUser(AdminSettingsViewModel model)
    {
        var requestUser = await FindUser();

        if (requestUser == default)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            _notyf.Error("User not found");

            return Json(new {success = false, message = "User not found"});
        }

        if (requestUser.UserRole != UserRole.Admin)
        {
            _notyf.Error("You are not authorized to create a user");

            return Json(new {success = false, message = "You are not authorized to create a user"});
        }

        if (string.IsNullOrWhiteSpace(model.NewUserUsername))
        {
            _notyf.Error("Username can't be empty.");

            return Json(new {success = false, message = "Username can't be empty."});
        }

        if (string.IsNullOrWhiteSpace(model.NewUserPassword))
        {
            _notyf.Error("Password can't be empty.");

            return Json(new {success = false, message = "Password can't be empty."});
        }

        PasswordManager.HashPassword(model.NewUserPassword, out var salt, out var hashedPassword);

        if (_context.Users.Any(x => x.Name.ToLower() == model.NewUserUsername.ToLower()))
        {
            _notyf.Error("The username already exists");

            return Json(new {success = false, message = "The username already exists"});
        }

        var user = new User
        {
            Name = model.NewUserUsername,
            Password = hashedPassword,
            Salt = salt
        };

        await _context.Users.AddAsync(user);

        var result = await _context.SaveChangesAsync();

        if (result != 1)
            _notyf.Error("The user can't be saved in the database");
        else
            _notyf.Success("User successfully created");

        var users = await _context.Users.Where(x => x.ID != 1).Select(x => new SelectListItem
        {
            Text = x.Name,
            Value = x.UniqueId.ToString()
        }).ToListAsync();

        model.Users = users;
        return Json(new {success = true, model});
    }

    [HttpPost]
    public async Task<JsonResult> ChangePassword(UserSettingsViewModel model)
    {
        var user = await FindUser();

        if (user == null)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            _notyf.Error("User not found");

            return Json(new {success = false, message = "User not found"});
        }

        var isPasswordCorrect = PasswordManager.CheckIfPasswordIsCorrect(model.Password, user.Salt, user.Password);

        if (!isPasswordCorrect)
        {
            _notyf.Error("The entered password was not correct");

            return Json(new {success = false, message = "The entered password was not correct"});
        }

        if (model.NewPassword != model.ConfirmNewPassword)
        {
            _notyf.Error("The new passwords don't match");

            return Json(new {success = false, message = "The new passwords don't match"});
        }

        PasswordManager.HashPassword(model.NewPassword, out var salt, out var hashedPassword);

        var lastChangedTime = DateTimeOffset.UtcNow;

        user.Password = hashedPassword;
        user.Salt = salt;
        user.LastChangedTicks = lastChangedTime.Ticks;

        await _context.SaveChangesAsync();

        await RefreshLogin(user, lastChangedTime);

        _notyf.Success("Password successfully changed");

        return Json(new {success = true, message = "Password successfully changed"});
    }

    private async Task RefreshLogin(User user, DateTimeOffset lastChangedTime)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Name),
            new("LastChanged", lastChangedTime.Ticks.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Role, user.UserRole.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            ExpiresUtc = lastChangedTime.AddMinutes(30),
            IssuedUtc = lastChangedTime
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authProperties);
    }

    [HttpPost]
    public async Task<JsonResult> ResetPasswordOfUser(AdminSettingsViewModel model)
    {
        PasswordManager.HashPassword(model.ResetUserPwNewPassword!, out var salt, out var password);

        var user = await _context.Users.FirstAsync(x => x.UniqueId.ToString() == model.SelectedUser);

        user.Password = password;
        user.Salt = salt;

        var result = await _context.SaveChangesAsync();

        if (result == 1)
            _notyf.Success($"{user.Name}'s password successfully reset");
        else
            _notyf.Error($"{user.Name}'s password couldn't be reset");

        model = new AdminSettingsViewModel();
        return Json(new {success = true, model});
    }

    [HttpPost]
    public async Task<JsonResult> ResetApiKey()
    {
        var user = await FindUser();

        if (user == null)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            _notyf.Error("User not found");

            return Json(new {success = false, message = "User not found"});
        }

        var newApiKey = Data.Entities.User.GenerateApiKey();

        user.ApiKey = newApiKey;

        var result = await _context.SaveChangesAsync();

        if (result == 1)
            _notyf.Success("Api key successfully reset");

        return Json(new {success = true, apiKey = newApiKey});
    }
}