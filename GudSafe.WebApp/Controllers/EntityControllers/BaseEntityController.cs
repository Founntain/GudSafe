using AutoMapper;
using GudSafe.Data;

namespace GudSafe.WebApp.Controllers.EntityControllers;

public class BaseEntityController<TController> : BaseController<TController>
{
    public BaseEntityController(GudSafeContext context, IMapper mapper, ILogger<TController> logger) : base(context,
        mapper, logger)
    {
    }
}