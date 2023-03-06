using System.Security.Claims;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GudSafe.WebApp.Classes.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class UserAccessAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var req = context.HttpContext.Request;

        User? user;

        if (context.HttpContext.User.Identity?.IsAuthenticated ?? false)
        {
            var name = context.HttpContext.User.FindFirstValue(ClaimTypes.Name);

            await using var db = new GudSafeContext();

            user = await db.Users.FirstOrDefaultAsync(x => x.Name == name);
        }
        else
        {
            var apiKey = req.Headers["apikey"].First();

            await using var db = new GudSafeContext();

            user = await db.Users.FirstOrDefaultAsync(x => x.ApiKey == apiKey);
        }

        if (user == default)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (user.UserRole >= UserRole.Default)
        {
            await next();
            return;
        }

        context.Result = new UnauthorizedResult();

        await next();
    }
}