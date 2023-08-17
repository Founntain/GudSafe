using RoverCore.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace GudSafe.WebApp.Controllers.ViewControllers;

public class BaseViewController : Controller
{
    public INotyfService Notyf { get; }

    public BaseViewController(INotyfService notyf)
    {
        Notyf = notyf;
    }
}