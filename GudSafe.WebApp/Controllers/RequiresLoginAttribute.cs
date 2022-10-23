using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GudSafe.WebApp.Controllers;

/// <summary>
/// Redirects all requests to the login page if the user is not logged in
/// </summary>
public class RequiresLoginAttribute : ActionFilterAttribute
{
    private readonly string _loginUrl;
    
    /// <summary>
    /// Creates a new <see cref="RequiresLoginAttribute"/> instance
    /// </summary>
    /// <param name="loginUrl">The login url to redirect to if not logged in</param>
    public RequiresLoginAttribute(string loginUrl)
    {
        _loginUrl = loginUrl;
    }
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        if (context.HttpContext.User.FindFirstValue(ClaimTypes.Name) == null)
        {
            context.Result = new RedirectResult(_loginUrl);
        }
    }
}