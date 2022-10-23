using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models.EntityModels;

namespace GudSafe.WebApp.Controllers.EnitityControllers;

public class GudCollectionController : BaseEntityController<GudCollectionController, GudCollection, GudCollectionModel>
{
    public GudCollectionController(GudSafeContext context, IMapper mapper, ILogger<GudCollectionController> logger) :
        base(context, mapper, logger)
    {
    }
}