using System.Globalization;
using System.Security.Claims;
using System.Text;
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

    public DashboardController(GudFileController fileController, GudSafeContext context, IMapper mapper,
        ILogger<DashboardController> logger)
    {
        _fileController = fileController;
        _context = context;
        _mapper = mapper;
        _logger = logger;
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

    public IActionResult PasswordChanged()
    {
        return View();
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
    public async Task<IActionResult> Delete(Guid id)
    {
        await _fileController.Delete(id);

        var user = await FindUser();

        if (user == default)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            return BadRequest("User not found");
        }

        var files = user.FilesUploaded;

        HttpContext.Response.StatusCode = 302;
        HttpContext.Response.Headers["Location"] = "/Dashboard/Gallery";

        return View("Gallery", new GalleryViewModel
        {
            Username = user.Name,
            Files = files.ToList()
        });
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
    public async Task<IActionResult> CreateUser([FromForm] AdminSettingsViewModel model)
    {
        var requestUser = await FindUser();

        if (requestUser == default)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            return BadRequest("User not found");
        }

        var users = await _context.Users.Where(x => x.ID != 1).Select(x => new SelectListItem
        {
            Text = x.Name,
            Value = x.UniqueId.ToString()
        }).ToListAsync();

        if (requestUser.UserRole != UserRole.Admin)
        {
            ModelState.AddModelError("NotAuthorizedCreate", "You are not authorized to create user");

            return View("AdminSettings", new AdminSettingsViewModel
            {
                Users = users
            });
        }

        if (string.IsNullOrWhiteSpace(model.NewUserUsername))
        {
            ModelState.AddModelError("UsernameNotExists", "Username can't be empty.");

            return View("AdminSettings", new AdminSettingsViewModel
            {
                Users = users
            });
        }

        if (string.IsNullOrWhiteSpace(model.NewUserPassword))
        {
            ModelState.AddModelError("PasswordNotExists", "Password can't be empty.");

            return View("AdminSettings", new AdminSettingsViewModel
            {
                Users = users
            });
        }

        PasswordManager.HashPassword(model.NewUserPassword, out var salt, out var hashedPassword);

        if (_context.Users.Any(x => x.Name.ToLower() == model.NewUserUsername.ToLower()))
        {
            ModelState.AddModelError("UsernameExists", "The username already exists");
        }
        else
        {
            var user = new User
            {
                Name = model.NewUserUsername,
                Password = hashedPassword,
                Salt = salt
            };

            await _context.Users.AddAsync(user);

            var result = await _context.SaveChangesAsync();

            if (result != 1)
                ModelState.AddModelError("CantSaveUserInDb", "The user can't be saved in the database");
            else
                ModelState.AddModelError("Success", "User successfully created");
        }

        users = await _context.Users.Where(x => x.ID != 1).Select(x => new SelectListItem
        {
            Text = x.Name,
            Value = x.UniqueId.ToString()
        }).ToListAsync();

        return View("AdminSettings", new AdminSettingsViewModel
        {
            Users = users
        });
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(UserSettingsViewModel model)
    {
        var user = await FindUser();

        if (user == null)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            return BadRequest("User not found");
        }

        var isPasswordCorrect = PasswordManager.CheckIfPasswordIsCorrect(model.Password, user.Salt, user.Password);

        if (!isPasswordCorrect)
        {
            ModelState.Clear();
            ModelState.AddModelError("PasswordNotCorrect", "The entered password was not correct");

            model.Password = "";
            model.NewPassword = "";
            model.ConfirmNewPassword = "";

            model.ApiKey = user.ApiKey;
            model.User = _mapper.Map<UserModel>(user);

            return View("UserSettings", model);
        }

        if (model.NewPassword != model.ConfirmNewPassword)
        {
            ModelState.Clear();
            ModelState.AddModelError("NewPasswordsDontMatch", "The new passwords don't match");

            model.Password = "";
            model.NewPassword = "";
            model.ConfirmNewPassword = "";

            model.ApiKey = user.ApiKey;
            model.User = _mapper.Map<UserModel>(user);

            return View("UserSettings", model);
        }

        PasswordManager.HashPassword(model.NewPassword, out var salt, out var hashedPassword);

        var lastChangedTime = DateTimeOffset.UtcNow;

        user.Password = hashedPassword;
        user.Salt = salt;
        user.LastChangedTicks = lastChangedTime.Ticks;

        await _context.SaveChangesAsync();

        await RefreshLogin(user, lastChangedTime);

        ModelState.Clear();
        ModelState.AddModelError("Success", "Password successfully changed");

        model.Password = "";
        model.NewPassword = "";
        model.ConfirmNewPassword = "";

        model.ApiKey = user.ApiKey;
        model.User = _mapper.Map<UserModel>(user);


        return RedirectToAction("PasswordChanged");
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

    public async Task<IActionResult> ResetPasswordOfUser([FromForm] AdminSettingsViewModel model)
    {
        PasswordManager.HashPassword(model.ResetUserPwNewPassword!, out var salt, out var password);

        var user = await _context.Users.FirstAsync(x => x.UniqueId.ToString() == model.SelectedUser);

        user.Password = password;
        user.Salt = salt;

        var result = await _context.SaveChangesAsync();

        if (result == 1)
            ModelState.AddModelError("Success", "Password successfully reset");
        else
            ModelState.AddModelError("Error", "Password couldn't be reset");

        var users = await _context.Users.Where(x => x.ID != 1).Select(x => new SelectListItem
        {
            Text = x.Name,
            Value = x.UniqueId.ToString()
        }).ToListAsync();

        return View("AdminSettings", new AdminSettingsViewModel
        {
            Users = users
        });
    }

    public async Task<IActionResult> ResetApiKey()
    {
        var user = await FindUser();

        if (user == null)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            return BadRequest("User not found");
        }

        var newApiKey = Data.Entities.User.GenerateApiKey();

        user.ApiKey = newApiKey;

        var result = await _context.SaveChangesAsync();

        if (result == 1)
            return View();

        return View("UserSettings");
    }
}