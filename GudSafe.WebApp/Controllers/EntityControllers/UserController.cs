using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models.EntityModels;

namespace GudSafe.WebApp.Controllers.EntityControllers;

public class UserController : BaseEntityController<UserController, User, UserModel>
{
    public UserController(GudSafeContext context, IMapper mapper, ILogger<UserController> logger) : base(context,
        mapper, logger)
    {
    }
    
}