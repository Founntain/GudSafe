using AutoMapper;
using GudSafe.Data;
using Microsoft.AspNetCore.Mvc;

namespace GudSafe.WebApp.Controllers.EntityControllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class BaseController<TController> : Controller
{
    protected readonly GudSafeContext Context;
    protected readonly IMapper Mapper;
    protected readonly ILogger<TController> Logger;

    public BaseController(GudSafeContext context, IMapper mapper, ILogger<TController> logger)
    {
        Context = context;
        Mapper = mapper;
        Logger = logger;
    }
}