using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models;

namespace GudSafe.WebApp.Controllers;

public class GudCollectionController : BaseEntityController<GudCollectionController, GudCollection, GudCollectionModel>
{
    public GudCollectionController(GudSafeContext context, IMapper mapper, ILogger<GudCollectionController> logger) :
        base(context, mapper, logger)
    {
    }
}