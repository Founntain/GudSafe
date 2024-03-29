using System.Globalization;
using System.Security.Claims;
using System.Text;
using RoverCore.ToastNotification.Abstractions;
using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Cryptography;
using GudSafe.Data.Entities;
using GudSafe.Data.Enums;
using GudSafe.Data.Models.EntityModels;
using GudSafe.Data.ViewModels;
using GudSafe.WebApp.Classes;
using GudSafe.WebApp.Classes.Attributes;
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
public class DashboardController : BaseViewController
{
    private readonly GudSafeContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(GudSafeContext context, IMapper mapper, ILogger<DashboardController> logger,
        INotyfService notyf) : base(notyf)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Gallery");
    }

    public async Task<IActionResult> Gallery(int page = 1)
    {
        var user = await FindUser();

        if (user == null) return BadRequest();

        var pageCount = (int) Math.Ceiling(user.FilesUploaded.Count / 18d);

        if (page < 1)
            return RedirectToAction("Gallery", new {page = 1});

        if (page > pageCount)
            return RedirectToAction("Gallery", new {page = pageCount});

        var pagedFiles = user.FilesUploaded.OrderByDescending(x => x.CreationTime).Skip((page - 1) * 18).Take(18);

        var view = View(new GalleryViewModel
        {
            Files = pagedFiles.ToList(),
            Page = page,
            TotalPages = pageCount
        });
        
        return view;
    }

    public async Task<IActionResult> UserSettings()
    {
        var user = await FindUser();

        var viewModel = new UserSettingsViewModel
        {
            User = _mapper.Map<UserModel>(user),
            ApiKey = user?.ApiKey ?? string.Empty
        };

        return View(viewModel);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminSettings()
    {
        var users = await _context.Users.Where(x => x.ID != 1).Select(x => new SelectListItem
        {
            Text = x.Name,
            Value = x.UniqueId.ToString()
        }).ToListAsync();

        var viewModel = new AdminSettingsViewModel
        {
            Users = users
        };

        return View(viewModel);
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
            RequestUrl = $"{Request.Scheme}://{Request.Host}/files/upload",
            Body = "MultipartFormData",
            Headers =
                new Dictionary<string, object>
                {
                    {"apikey", user.ApiKey}
                },
            FileFormName = "file",
            Url = "$json:url$",
            ThumbnailUrl = "$json:thumbnailUrl$"
        };

        var json = JsonConvert.SerializeObject(shareXProfile);

        var data = Encoding.UTF8.GetBytes(json);

        return File(data, "application/octet-stream", $"GudSafe-{Request.Host}.sxcu");
    }

    [HttpPost]
    public async Task<JsonResult> UploadFile()
    {
        if (Request.ContentType == null || !Request.ContentType.StartsWith("multipart/form-data"))
            return Json(new {success = false});

        var curChunk = int.Parse(Request.Form["chunk"].ToString());
        var chunkCount = int.Parse(Request.Form["chunkCount"].ToString());
        var isFirst = !Guid.TryParse(Request.Form["uploadId"].ToString(), out var uploadId);

        var file = Request.Form.Files[0];

        var user = await FindUser();

        if (user == default)
            return Json(new {success = false});

        GudFile gudFile;

        if (isFirst)
        {
            gudFile = (await GudFileController.UploadFile(file, user, _context)).Entity;
        }
        else
        {
            gudFile = await _context.Files.FirstAsync(x => x.UniqueId == uploadId);

            await GudFileController.UploadChunk(file, gudFile, curChunk, _context);
        }

        if (curChunk == chunkCount - 1)
        {
            await GudFileController.GenerateThumbnail(gudFile);

            var success = bool.TryParse(Environment.GetEnvironmentVariable("FORCE_HTTPS"), out var result);

            var url = success && result
                ? $"https://{Request.Host}/f/{gudFile.ShortUrl}.{gudFile.FileExtension}"
                : $"{Request.Scheme}://{Request.Host}/f/{gudFile.ShortUrl}.{gudFile.FileExtension}";

            Notyf.Success(
                $"File uploaded successfully.\n Open <a href=\"{url}\" target=\"_blank\">here</a>!",
                4);
        }

        return Json(new {success = true, uploadId = gudFile.UniqueId.ToString()});
    }

    [HttpPost]
    public async Task<JsonResult> DeleteFile(Guid id)
    {
        var user = await FindUser();

        if (user == default)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            Notyf.Error("User not found");

            return Json(new {success = false});
        }

        var file = user.FilesUploaded.FirstOrDefault(x => x.UniqueId == id);

        if (file == default)
            return Json(new {success = false});

        GudFileController.DeleteFileFromDrive(file, _logger);

        _context.Files.Remove(file);

        await _context.SaveChangesAsync();

        Notyf.Success($"File {id} deleted successfully", 2);

        return Json(new {success = true});
    }

    private Task<User?> FindUser()
    {
        return _context.Users
            .FirstOrDefaultAsync(x => x.Name == User.FindFirstValue(ClaimTypes.Name));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<JsonResult> CreateUser(AdminSettingsViewModel model)
    {
        var requestUser = await FindUser();

        if (requestUser == default)
        {
            _logger.LogWarning("The logged in user {Name} wasn't found in the database",
                User.FindFirstValue(ClaimTypes.Name));

            Notyf.Error("User not found");

            return Json(new {success = false, message = "User not found"});
        }

        if (requestUser.UserRole != UserRole.Admin)
        {
            Notyf.Error("You are not authorized to create a user");

            return Json(new {success = false, message = "You are not authorized to create a user"});
        }

        if (string.IsNullOrWhiteSpace(model.NewUserUsername))
        {
            Notyf.Error("Username can't be empty.");

            return Json(new {success = false, message = "Username can't be empty."});
        }

        if (string.IsNullOrWhiteSpace(model.NewUserPassword))
        {
            Notyf.Error("Password can't be empty.");

            return Json(new {success = false, message = "Password can't be empty."});
        }

        PasswordManager.HashPassword(model.NewUserPassword, out var salt, out var hashedPassword);

        if (_context.Users.Any(x => x.Name.ToLower() == model.NewUserUsername.ToLower()))
        {
            Notyf.Error("The username already exists");

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
            Notyf.Error("The user can't be saved in the database");
        else
            Notyf.Success("User successfully created");

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

            Notyf.Error("User not found");

            return Json(new {success = false, message = "User not found"});
        }

        var isPasswordCorrect = PasswordManager.CheckIfPasswordIsCorrect(model.Password, user.Salt, user.Password);

        if (!isPasswordCorrect)
        {
            Notyf.Error("The entered password was not correct");

            return Json(new {success = false, message = "The entered password was not correct"});
        }

        if (model.NewPassword != model.ConfirmNewPassword)
        {
            Notyf.Error("The new passwords don't match");

            return Json(new {success = false, message = "The new passwords don't match"});
        }

        PasswordManager.HashPassword(model.NewPassword, out var salt, out var hashedPassword);

        var lastChangedTime = DateTimeOffset.UtcNow;

        user.Password = hashedPassword;
        user.Salt = salt;
        user.LastChangedTicks = lastChangedTime.Ticks;

        await _context.SaveChangesAsync();

        await RefreshLogin(user, lastChangedTime);

        Notyf.Success("Password successfully changed");

        return Json(new {success = true, message = "Password successfully changed"});
    }

    [NonAction]
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
    [Authorize(Roles = "Admin")]
    public async Task<JsonResult> ResetPasswordOfUser(AdminSettingsViewModel model)
    {
        PasswordManager.HashPassword(model.ResetUserPwNewPassword!, out var salt, out var password);

        var user = await _context.Users.FirstAsync(x => x.UniqueId.ToString() == model.SelectedUser);

        user.Password = password;
        user.Salt = salt;

        var result = await _context.SaveChangesAsync();

        if (result == 1)
            Notyf.Success($"{user.Name}'s password successfully reset");
        else
            Notyf.Error($"{user.Name}'s password couldn't be reset");

        return Json(new {success = true, model});
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<JsonResult> DeleteUser(AdminSettingsViewModel model)
    {
        var uid = Guid.TryParse(model.SelectedUser, out var guid) ? guid : Guid.Empty;

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UniqueId == uid);

        if (user == null)
        {
            _logger.LogWarning("The user with id {Name} wasn't found in the database",
                model.SelectedUser);

            Notyf.Error("The user couldn't be found in the database");

            return Json(new {success = false});
        }

        foreach (var gudFile in user.FilesUploaded)
        {
            GudFileController.DeleteFileFromDrive(gudFile, _logger);
        }

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        var users = await _context.Users.Where(x => x.ID != 1).Select(x => new SelectListItem
        {
            Text = x.Name,
            Value = x.UniqueId.ToString()
        }).ToListAsync();

        model.Users = users;

        Notyf.Success($"User '{user.Name}' successfully deleted");

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

            Notyf.Error("User not found");

            return Json(new {success = false, message = "User not found"});
        }

        var newApiKey = Data.Entities.User.GenerateApiKey();

        user.ApiKey = newApiKey;

        var result = await _context.SaveChangesAsync();

        if (result == 1)
            Notyf.Success("Api key successfully reset");

        return Json(new {success = true, apiKey = newApiKey});
    }
}