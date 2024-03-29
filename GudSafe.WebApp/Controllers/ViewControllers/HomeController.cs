﻿using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using RoverCore.ToastNotification.Abstractions;
using GudSafe.Data;
using GudSafe.Data.Cryptography;
using GudSafe.Data.ViewModels;
using GudSafe.WebApp.Classes;
using GudSafe.WebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace GudSafe.WebApp.Controllers.ViewControllers;

public class HomeController : BaseViewController
{
    private readonly GudSafeContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(GudSafeContext context, ILogger<HomeController> logger, INotyfService notyf) : base(notyf)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Login()
    {
        if (HttpContext.User.FindFirstValue(ClaimTypes.Name) != null)
            return RedirectToAction("Index");

        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> Login(LoginViewModel loginModel, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(loginModel.Name) || string.IsNullOrWhiteSpace(loginModel.Password))
        {
            Notyf.Error("Either the username or password were empty.");

            return Json(new {success = false});
        }

        var user = _context.Users.FirstOrDefault(x => x.Name == loginModel.Name);

        if (user == null)
        {
            _logger.LogError("User {User} not found in db", loginModel.Name);

            Notyf.Error("Username or password incorrect.");

            return Json(new {success = false});
        }

        var success = PasswordManager.CheckIfPasswordIsCorrect(loginModel.Password, user.Salt, user.Password);

        if (!success)
        {
            Notyf.Error("Username or password incorrect.");

            return Json(new {success = false, redirectUrl = $"{Url.Action("Login")}"});
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UniqueId.ToString()),
            new(ClaimTypes.Name, user.Name),
            new("LastChanged", user.LastChangedTicks.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Role, user.UserRole.ToString())
        };

        var claimsIdent = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            IssuedUtc = DateTimeOffset.UtcNow
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdent),
            authProperties);

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            return Json(new {success = true, redirectUrl = returnUrl});
        }

        return Json(new {success = true, redirectUrl = $"{Url.Action("Index")}"});
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!Request.IsAjax())
            return RedirectToAction("Index");

        return Json(new {redirectUrl = $"{Url.Action("Index")}"});
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}