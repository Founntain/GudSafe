using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GudSafe.WebApp.Classes;

public static class AjaxRequestExtensions
{
    public static bool IsAjax(this HttpRequest request)
    {
        return request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }

    public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model,
        bool partial = false)
    {
        if (string.IsNullOrEmpty(viewName))
        {
            viewName = controller.ControllerContext.ActionDescriptor.ActionName;
        }

        controller.ViewData.Model = model;

        await using (var writer = new StringWriter())
        {
            var viewEngine = controller.HttpContext.RequestServices.GetService<ICompositeViewEngine>();

            var viewResult = viewEngine?.FindView(controller.ControllerContext, viewName, !partial);

            if (!viewResult?.Success ?? true)
            {
                return $"A view with the name {viewName} could not be found";
            }

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);

            return writer.GetStringBuilder().ToString();
        }
    }
}