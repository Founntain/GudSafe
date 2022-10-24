using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models.EntityModels;
using GudSafe.WebApp.Classes.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace GudSafe.WebApp.Controllers.EnitityControllers;

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

        HttpContext.Response.Headers.Add("Content-Disposition", $"filename={file.Name}");

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
    [UserAccess]
    [Route("upload")]
    // TODO: Maybe Admin Customizable number
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var token = Request.Headers["apikey"].First();

        var user = await _context.Users.FirstAsync(x => x.ApiKey == token);

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
            using var bitmap = new SKBitmap();
            using var surface = SKSurface.Create(new SKImageInfo(200, 200));
            using var paint = new SKPaint();

            paint.Color = SKColors.White;
            paint.TextAlign = SKTextAlign.Center;
            paint.TextSize = 36;
            paint.IsAntialias = true;
            paint.FilterQuality = SKFilterQuality.High;

            surface.Canvas.DrawColor(new SKColor(255, 255, 255, 25));
            surface.Canvas.DrawText($".{newFile.FileExtension}", 100, 109, paint);
            surface.Canvas.Flush();

            using var data = surface.Snapshot().Encode(SKEncodedImageFormat.Webp, 75);

            newFile.ThumbnailData = data.ToArray();
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