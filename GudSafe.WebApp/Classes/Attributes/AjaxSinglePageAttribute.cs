using GudSafe.WebApp.Controllers.ViewControllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GudSafe.WebApp.Classes.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AjaxSinglePageAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.IsAjax())
        {
            if (context.Controller is not BaseViewController controller) return;

            context.Result = controller.View("Main");
        }
    }
}