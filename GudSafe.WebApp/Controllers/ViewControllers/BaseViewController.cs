using AspNetCoreHero.ToastNotification.Abstractions;
using GudSafe.WebApp.Classes.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace GudSafe.WebApp.Controllers.ViewControllers;

[AjaxSinglePage]
public class BaseViewController : Controller
{
    public INotyfService Notyf { get; }

    public BaseViewController(INotyfService notyf)
    {
        Notyf = notyf;
    }
}