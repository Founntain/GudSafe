using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models.EntityModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GudSafe.WebApp.Controllers.EnitityControllers;

public class BaseEntityController<TController, TEntity, TModel> : BaseController<TController>
    where TEntity : BaseEntity
    where TModel : BaseModel
{
    public BaseEntityController(GudSafeContext context, IMapper mapper, ILogger<TController> logger) : base(context,
        mapper, logger)
    {
    }

    [HttpGet]
    [Route("get")]
    public virtual async Task<ActionResult> Get()
    {
        // Get all entities from the database
        var data = await _context.Set<TEntity>().ToListAsync();

        // Map the entities to models and return them
        var mappedData = _mapper.Map<ICollection<TModel>>(data);

        return Ok(mappedData);
    }

    [HttpGet]
    [Route("getById")]
    public async Task<ActionResult> GetById(Guid id)
    {
        // Get the entity from the database
        var data = await _context.Set<TEntity>().FirstOrDefaultAsync(x => x.UniqueId == id);

        // Check if the entity exists, if not return 404
        if (data is null)
            return NotFound();

        // Map the entity to a model and return it
        var mappedData = _mapper.Map<TModel>(data);

        return Ok(mappedData);
    }
}