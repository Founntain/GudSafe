using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models;

namespace GudSafe.WebApp.Controllers;

public class GudFileController : BaseEntityController<GudFileController, GudFile, GudFileModel>
{
    public GudFileController(GudSafeContext context, IMapper mapper, ILogger<GudFileController> logger) : base(context,
        mapper, logger)
    {
    }
}