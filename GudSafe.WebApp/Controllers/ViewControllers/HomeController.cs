using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using GudSafe.Data;
using GudSafe.Data.ViewModels;
using GudSafe.WebApp.Classes.Cryptography;
using Microsoft.AspNetCore.Mvc;
using GudSafe.WebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GudSafe.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly GudSafeContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(GudSafeContext context, ILogger<HomeController> logger)
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel loginModel, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(loginModel.Name) || string.IsNullOrWhiteSpace(loginModel.Password))
            return BadRequest();

        var context = new GudSafeContext();

        var user = context.Users.FirstOrDefault(x => x.Name == loginModel.Name);

        if (user == null)
            return BadRequest();

        var success = PasswordManager.CheckIfPasswordIsCorrect(loginModel.Password, user.Salt, user.Password);

        if (!success)
            return View(loginModel);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Name),
            new("LastChanged", user.LastChangedTicks.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Role, "User")
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
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}