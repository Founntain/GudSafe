using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models;
using GudSafe.Data.Models.RequestModels;
using GudSafe.WebApp.Classes.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace GudSafe.WebApp.Controllers;

public class UserController : BaseEntityController<UserController, User, UserModel>
{
    public UserController(GudSafeContext context, IMapper mapper, ILogger<UserController> logger) : base(context,
        mapper, logger)
    {
    }

    [HttpPost]
    [Route("create")]
    public async Task<ActionResult> Create([FromBody] CreateUserModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name)
            || string.IsNullOrWhiteSpace(model.Password))
            return BadRequest("Please provide a Name and Password");

        var hashedPassword = PasswordManager.HashPassword(model.Password);

        var user = new User
        {
            Name = model.Name,
            Password = hashedPassword.Item2,
            Salt = hashedPassword.Item1
        };

        await _context.Users.AddAsync(user);

        await _context.SaveChangesAsync();
        
        return Ok(user);
    }
}