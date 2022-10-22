using AutoMapper;
using GudSafe.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GudSafe.WebApp.Controllers;

public class DeleteController : BaseController<DeleteController>
{
    public DeleteController(GudSafeContext context, IMapper mapper, ILogger<DeleteController> logger) : base(context, mapper, logger)
    {
    }
    
    [HttpPost]
    [Route("delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Please supply a valid ID");

        var fileToDelete = await _context.Files.FirstOrDefaultAsync(x => x.UniqueId == id);

        if (fileToDelete == default)
            return NotFound("The file you try to delete wasn't found");

        _context.Files.Remove(fileToDelete);

        var result = await _context.SaveChangesAsync();

        return result == 1 ? Ok() : BadRequest("File could not be deleted, please try again alter");
    }
}