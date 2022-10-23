using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace GudSafe.WebApp.Controllers;

[Route("api/files")]
public class GudFileController : BaseEntityController<GudFileController, GudFile, GudFileModel>
{
    public GudFileController(GudSafeContext context, IMapper mapper, ILogger<GudFileController> logger) : base(context,
        mapper, logger)
    {
    }

    [HttpGet]
    [Route("{name}")]
    public async Task<ActionResult> Get(string name)
    {
        var file = await _context.Files.FirstOrDefaultAsync(x => x.UniqueId == Guid.Parse(name));

        if (file == null)
            return NotFound();

        return File(file.FileData, file.FileType);
    }

    [HttpGet]
    [Route("{name}/thumbnail")]
    public async Task<ActionResult> GetThumb(string name)
    {
        var file = await _context.Files.FirstOrDefaultAsync(x => x.UniqueId == Guid.Parse(name));

        if (file == null)
            return NotFound();

        return File(file.ThumbnailData, "image/webp");
    }

    [HttpPost]
    [Route("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var requestHeaders = Request.Headers;

        var token = requestHeaders["apikey"].First();

        if (string.IsNullOrEmpty(token))
            return BadRequest("API key is empty or not provided");

        var user = _context.Users.FirstOrDefault(x => x.ApiKey == token);

        if (user == null)
            return NotFound("The user with the provided API key was not found");

        await using var memStream = new MemoryStream();

        await file.CopyToAsync(memStream);

        var fileBytes = memStream.ToArray();

        var newFile = new GudFile
        {
            Creator = user,
            FileData = fileBytes,
            FileExtension = Path.GetExtension(file.FileName)[1..],
            FileType = file.ContentType,
            Name = file.FileName
        };

        if (file.ContentType.Contains("image"))
        {
            using var bitmap = SKBitmap.Decode(fileBytes);

            var ratio = Math.Max(bitmap.Width / 200d, bitmap.Height / 200d);

            using var scaled = bitmap.Resize(
                new SKImageInfo((int) (bitmap.Width / ratio), (int) (bitmap.Height / ratio)),
                SKFilterQuality.Medium);
            using var data = scaled.Encode(SKEncodedImageFormat.Webp, 75);

            newFile.ThumbnailData = data.ToArray();
        }
        else
        {
            newFile.ThumbnailData = Array.Empty<byte>();
        }

        var newEntry = await _context.Files.AddAsync(newFile);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Url = $"{Request.Scheme}://{Request.Host}/api/files/{newEntry.Entity.UniqueId}",
            ThumbnailUrl = $"{Request.Scheme}://{Request.Host}/api/files/{newEntry.Entity.UniqueId}/thumbnail"
        });
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