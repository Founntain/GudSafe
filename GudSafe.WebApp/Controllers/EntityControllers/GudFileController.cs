using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.Data.Models.EntityModels;
using GudSafe.WebApp.Classes.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace GudSafe.WebApp.Controllers.EntityControllers;

[Route("files")]
public class GudFileController : BaseEntityController<GudFileController, GudFile, GudFileModel>
{
    public static readonly string ImagesPath = "gudfiles";
    public static readonly string ThumbnailsPath = Path.Combine(ImagesPath, "thumbnails");

    public GudFileController(GudSafeContext context, IMapper mapper, ILogger<GudFileController> logger) : base(context,
        mapper, logger)
    {
    }

    [HttpGet]
    [Route("{name}")]
    public async Task<ActionResult> Get(string name)
    {
        var guid = Guid.Parse(name);
        var dbFile = await _context.Files.FirstOrDefaultAsync(x => x.UniqueId == guid);

        if (dbFile == null)
            return NotFound();

        try
        {
            var file = System.IO.File.Open($"{Path.Combine(ImagesPath, $"{name}.{dbFile.FileExtension}")}",
                FileMode.Open);

            HttpContext.Response.Headers.Add("Content-Disposition", $"filename={dbFile.Name}");

            return File(file, dbFile.FileType);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            return NotFound("The requested file was not found, was it deleted or moved?");
        }
    }

    [HttpGet]
    [Route("{name}/thumbnail")]
    public async Task<ActionResult> GetThumb(string name)
    {
        var guid = Guid.Parse(name);
        var dbFile = await _context.Files.FirstOrDefaultAsync(x => x.UniqueId == guid);

        if (dbFile == null)
            return NotFound();

        try
        {
            var file = System.IO.File.Open($"{Path.Combine(ThumbnailsPath, name)}", FileMode.Open);

            HttpContext.Response.Headers.Add("Content-Disposition", $"filename={dbFile.Name}");

            return File(file, "image/webp");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            return NotFound("The requested file was not found, was it deleted or moved?");
        }
    }

    [HttpPost]
    [UserAccess]
    [Route("upload")]
    // TODO: Maybe Admin Customizable number
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = 1024L * 1024L * 500L)]
    public async Task<IActionResult> UploadFile()
    {
        if (Request.ContentType == null || !Request.ContentType.StartsWith("multipart/form-data"))
            return BadRequest();

        var file = Request.Form.Files[0];

        var token = Request.Headers["apikey"].First();

        var user = await _context.Users.FirstAsync(x => x.ApiKey == token);

        await using var stream = file.OpenReadStream();

        var newFile = new GudFile
        {
            Creator = user,
            FileExtension = Path.GetExtension(file.FileName)[1..],
            FileType = file.ContentType,
            Name = file.FileName
        };

        var newEntry = await _context.Files.AddAsync(newFile);

        var imagePath = Path.Combine(ImagesPath, $"{newFile.UniqueId}.{newFile.FileExtension}");
        var thumbnailPath = Path.Combine(ThumbnailsPath, newFile.UniqueId.ToString());

        Directory.CreateDirectory(ThumbnailsPath);

        await using var imageFs = new FileStream(imagePath, FileMode.Create);
        await stream.CopyToAsync(imageFs);

        if (file.ContentType.Contains("image"))
        {
            imageFs.Seek(0, SeekOrigin.Begin);
            using var bitmap = SKBitmap.Decode(imageFs);

            var ratio = Math.Max(bitmap.Width / 200d, bitmap.Height / 200d);

            using var scaled = bitmap.Resize(
                new SKImageInfo((int) (bitmap.Width / ratio), (int) (bitmap.Height / ratio)),
                SKFilterQuality.Medium);
            using var data = scaled.Encode(SKEncodedImageFormat.Webp, 75);

            await using var fs = System.IO.File.OpenWrite(thumbnailPath);

            await data.AsStream().CopyToAsync(fs);
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

            await using var fs = System.IO.File.OpenWrite(thumbnailPath);

            await data.AsStream().CopyToAsync(fs);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Url = $"{Request.Scheme}://{Request.Host}/files/{newEntry.Entity.UniqueId}",
            ThumbnailUrl = $"{Request.Scheme}://{Request.Host}/files/{newEntry.Entity.UniqueId}/thumbnail"
        });
    }

    [HttpPost]
    [Route("delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Please supply a valid ID");

        var thumbnailPath = Path.Combine(ThumbnailsPath, id.ToString());
        
        if (System.IO.File.Exists(thumbnailPath))
            System.IO.File.Delete(thumbnailPath);

        var fileToDelete = await _context.Files.FirstOrDefaultAsync(x => x.UniqueId == id);

        if (fileToDelete == default)
            return NotFound("The file you try to delete wasn't found");

        var imagePath = Path.Combine(ImagesPath, $"{id}.{fileToDelete.FileExtension}");
        
        if (System.IO.File.Exists(imagePath))
            System.IO.File.Delete(imagePath);

        _context.Files.Remove(fileToDelete);

        var result = await _context.SaveChangesAsync();

        return result == 1 ? Ok() : BadRequest("File could not be deleted, please try again alter");
    }
}