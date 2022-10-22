using AutoMapper;
using GudSafe.Data;
using Microsoft.AspNetCore.Mvc;

namespace GudSafe.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BaseController<TController> : Controller
{
    protected readonly GudSafeContext _context;
    protected readonly IMapper _mapper;
    private readonly ILogger<TController> _logger;

    public BaseController(GudSafeContext context, IMapper mapper, ILogger<TController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }
}