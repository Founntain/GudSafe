using GudSafe.WebApp.Classes.GithubUpdater;
using GudSafe.WebApp.Controllers.ViewControllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GudSafe.WebApp.Classes.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AjaxSinglePageAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.Any(x => x is IgnoreAjaxAttribute))
        {
            return;
        }

        if (context.HttpContext.Request.IsAjax()) return;

        if (context.Controller is not BaseViewController controller) return;

        var githubUpdateService = context.HttpContext.RequestServices.GetService<GithubUpdateService>();

        if ((githubUpdateService?.LatestResponse.IsNewVersionAvailable ?? false) && context.HttpContext.User.IsInRole("Admin"))
        {
            controller.Notyf.Information($"A new version of the gudsafe is available. You can find it on <a href=\"{githubUpdateService.LatestResponse.HtmlUrl}\" target=\"_blank\">Github</a>", 9999);
        }

        context.Result = controller.View("Main");
    }
}