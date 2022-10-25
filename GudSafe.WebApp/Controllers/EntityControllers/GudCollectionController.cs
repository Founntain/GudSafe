using AutoMapper;
using GudSafe.Data;

namespace GudSafe.WebApp.Controllers.EntityControllers;

public class GudCollectionController : BaseEntityController<GudCollectionController>
{
    public GudCollectionController(GudSafeContext context, IMapper mapper, ILogger<GudCollectionController> logger) :
        base(context, mapper, logger)
    {
    }
}