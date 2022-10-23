using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Cryptography;
using GudSafe.Data.Entities;
using GudSafe.Data.Models.EntityModels;
using GudSafe.Data.Models.RequestModels;
using GudSafe.Data.ViewModels;
using GudSafe.WebApp.Classes.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace GudSafe.WebApp.Controllers.EnitityControllers;

public class UserController : BaseEntityController<UserController, User, UserModel>
{
    public UserController(GudSafeContext context, IMapper mapper, ILogger<UserController> logger) : base(context,
        mapper, logger)
    {
    }

    [HttpPost]
    [Route("create")]
    [AdminAccess]
    public async Task<ActionResult> Create([FromBody] CreateUserModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name)
            || string.IsNullOrWhiteSpace(model.Password))
            return BadRequest("Please provide a Name and Password");

        PasswordManager.HashPassword(model.Password, out var salt, out var hashedPassword);

        if (_context.Users.Any(x => x.Name.ToLower() == model.Name.ToLower()))
        {
            return BadRequest($"User with the name {model.Name} already exists, please choose another username");
        }

        var user = new User
        {
            Name = model.Name,
            Password = hashedPassword,
            Salt = salt
        };

        await _context.Users.AddAsync(user);

        var result = await _context.SaveChangesAsync();

        return result == 1 ? Ok(user) : BadRequest("Couldn't save user in Database");
    }

    [HttpPost]
    [Route("createFromUi")]
    public async Task<IActionResult> CreateFromUi([FromForm] AdminSettingsViewModel model)
    {
        Request.Headers["apikey"] = model.ApiKey;

        return await Create(new CreateUserModel()
        {
            Name = model.NewUserUsername,
            Password = model.NewUserPassword
        });
    }
}