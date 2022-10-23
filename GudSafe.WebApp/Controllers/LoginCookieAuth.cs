using System.Security.Claims;
using GudSafe.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GudSafe.WebApp.Controllers;

public class LoginCookieAuth : CookieAuthenticationEvents
{
    private readonly GudSafeContext _context;

    public LoginCookieAuth(GudSafeContext context)
    {
        _context = context;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var user = context.Principal;

        var lastChanged = user?.FindFirstValue("LastChanged");
        var userName = user?.FindFirstValue(ClaimTypes.Name);

        if (!_context.Users.Any())
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return;
        }

        var dbUser = _context.Users.FirstOrDefault(x => x.Name == userName);

        if (string.IsNullOrWhiteSpace(lastChanged) || dbUser == null)
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return;
        }

        long time;

        try
        {
            time = long.Parse(lastChanged);
        }
        catch (Exception)
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return;
        }

        if (time != dbUser.LastChangedTicks)
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}