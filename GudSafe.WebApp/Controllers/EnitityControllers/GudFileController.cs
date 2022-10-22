using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models;
using Microsoft.AspNetCore.Mvc;
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
    public ActionResult Get(string name)
    {
        var file = _context.Files.FirstOrDefault(x => x.UniqueId == Guid.Parse(name));

        if (file == null)
            return NotFound();

        return File(file.FileData, file.FileType);
    }

    [HttpGet]
    [Route("{name}/thumbnail")]
    public ActionResult GetThumb(string name)
    {
        var file = _context.Files.FirstOrDefault(x => x.UniqueId == Guid.Parse(name));

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

        using var bitmap = SKBitmap.Decode(fileBytes);
        using var scaled = bitmap.Resize(new SKImageInfo(bitmap.Width / 4, bitmap.Height / 4), SKFilterQuality.Medium);
        using var data = scaled.Encode(SKEncodedImageFormat.Webp, 75);

        var newEntry = await _context.Files.AddAsync(new GudFile
        {
            Creator = user,
            FileData = fileBytes,
            FileExtension = Path.GetExtension(file.FileName)[1..],
            FileType = file.ContentType,
            ThumbnailData = data.ToArray(),
            Name = file.FileName
        });

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Url = $"{Request.Scheme}://{Request.Host}/api/files/{newEntry.Entity.UniqueId}",
            ThumbnailUrl = $"{Request.Scheme}://{Request.Host}/api/files/{newEntry.Entity.UniqueId}/thumbnail"
        });
    }
}