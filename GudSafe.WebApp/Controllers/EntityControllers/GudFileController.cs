using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.WebApp.Classes.Attributes;
using GudSafe.WebApp.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace GudSafe.WebApp.Controllers.EntityControllers;

[Route("files")]
public class GudFileController : BaseEntityController<GudFileController>
{
    public static readonly string ImagesPath = "gudfiles";
    public static readonly string ThumbnailsPath = Path.Combine(ImagesPath, "thumbnails");

    private readonly IHubContext<UploadHub> _uploadHub;

    public GudFileController(GudSafeContext context, IMapper mapper, ILogger<GudFileController> logger,
        IHubContext<UploadHub> uploadHub) : base(context,
        mapper, logger)
    {
        _uploadHub = uploadHub;
    }

    [HttpGet]
    [Route("{name}")]
    public async Task<ActionResult> Get(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("File name is empty");

        var parts = name.Split('.');

        Guid guid;

        try
        {
            guid = Guid.Parse(parts[0]);
        }
        catch (Exception)
        {
            Logger.LogWarning("Failed formatting {Name} to guid", name);

            return NotFound("Couldn't find the requested file");
        }

        var dbFile = await Context.Files.FirstOrDefaultAsync(x => x.UniqueId == guid);

        if (dbFile == null)
            return NotFound();

        if (parts.Length != 2)
            return RedirectPermanent($"/files/{dbFile.UniqueId}.{dbFile.FileExtension}");

        if (dbFile.FileExtension != parts[1])
            return NotFound("Couldn't find the requested file");

        var path = Path.Combine(ImagesPath, $"{guid}.{dbFile.FileExtension}");

        try
        {
            var file = System.IO.File.Open(path, FileMode.Open);

            HttpContext.Response.Headers.Add("Content-Disposition", $"filename={dbFile.Name}");
            HttpContext.Response.Headers.CacheControl = "public, max-age=3600";

            return File(file, dbFile.FileType);
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "The file {Name} was not found", path);

            return NotFound("The requested file was not found, was it deleted or moved?");
        }
    }

    [HttpGet]
    [Route("{name}/thumbnail")]
    public async Task<ActionResult> GetThumb(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("File name is empty");

        var parts = name.Split('.');

        Guid guid;

        try
        {
            guid = Guid.Parse(parts[0]);
        }
        catch (Exception)
        {
            Logger.LogWarning("Failed formatting {Name} to guid", parts[0]);

            return NotFound("Couldn't find the requested file");
        }

        var dbFile = await Context.Files.FirstOrDefaultAsync(x => x.UniqueId == guid);

        if (dbFile == null)
            return NotFound();

        if (parts.Length == 2 && dbFile.FileExtension != parts[1])
            return NotFound("Couldn't find the requested file");

        var path = Path.Combine(ThumbnailsPath, $"{guid}");

        try
        {
            var file = System.IO.File.Open(path, FileMode.Open);

            HttpContext.Response.Headers.Add("Content-Disposition", $"filename={dbFile.Name}");
            HttpContext.Response.Headers.CacheControl = "public, max-age=15552000";

            return File(file, "image/webp");
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "The file {Name} was not found", path);

            return NotFound("The requested file was not found, was it deleted or moved?");
        }
    }

    [HttpPost]
    [UserAccess]
    [Route("upload")]
    [BodyLimit]
    public async Task<IActionResult> UploadFile()
    {
        if (Request.ContentType == null || !Request.ContentType.StartsWith("multipart/form-data"))
            return BadRequest();

        var file = Request.Form.Files[0];

        var token = Request.Headers["apikey"].First();

        var user = await Context.Users.FirstAsync(x => x.ApiKey == token);

        await using var stream = file.OpenReadStream();

        var newFile = new GudFile
        {
            Creator = user,
            FileExtension = Path.GetExtension(file.FileName)[1..],
            FileType = file.ContentType,
            Name = file.FileName
        };

        var newEntry = await Context.Files.AddAsync(newFile);

        var imagePath = Path.Combine(ImagesPath, $"{newFile.UniqueId}.{newFile.FileExtension}");
        var thumbnailPath = Path.Combine(ThumbnailsPath, newFile.UniqueId.ToString());

        Directory.CreateDirectory(ThumbnailsPath);

        await using var imageFs = new FileStream(imagePath, FileMode.Create);
        await stream.CopyToAsync(imageFs);

        if (file.ContentType.Contains("image"))
        {
            await ImageToThumbnail(stream, thumbnailPath, newFile);
        }
        else
        {
            await ExtensionToThumbnail(newFile, thumbnailPath);
        }

        await Context.SaveChangesAsync();

        await _uploadHub.Clients.User(user.UniqueId.ToString()).SendAsync("RefreshFiles");

        return Ok(new
        {
            Url = $"{Request.Scheme}://{Request.Host}/files/{newEntry.Entity.UniqueId}.{newEntry.Entity.FileExtension}",
            ThumbnailUrl =
                $"{Request.Scheme}://{Request.Host}/files/{newEntry.Entity.UniqueId}.{newEntry.Entity.FileExtension}/thumbnail"
        });
    }

    private static async Task ImageToThumbnail(Stream stream, string thumbnailPath, GudFile newFile)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using var bitmap = SKBitmap.Decode(stream);

        if (bitmap == null)
        {
            await ExtensionToThumbnail(newFile, thumbnailPath);

            return;
        }

        var ratio = Math.Max(bitmap.Width / 200d, bitmap.Height / 200d);

        using var scaled = bitmap.Resize(
            new SKImageInfo((int) (bitmap.Width / ratio), (int) (bitmap.Height / ratio)), SKFilterQuality.Medium);
        using var data = scaled.Encode(SKEncodedImageFormat.Webp, 75);

        await using var fs = System.IO.File.OpenWrite(thumbnailPath);

        await data.AsStream().CopyToAsync(fs);
    }

    private static async Task ExtensionToThumbnail(GudFile newFile, string thumbnailPath)
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

    [HttpPost]
    [Route("delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Please supply a valid ID");

        var apiKey = Request.Headers["apikey"].FirstOrDefault();

        if (apiKey == null)
            return Unauthorized();

        var fileToDelete = await Context.Files.FirstOrDefaultAsync(x => x.UniqueId == id);

        if (fileToDelete == default)
            return new NotFoundObjectResult("The file you try to delete wasn't found");

        var canDelete = fileToDelete.Creator.ApiKey == apiKey;

        if (!canDelete)
            return Unauthorized();

        DeleteFileFromDrive(fileToDelete, Logger);

        Context.Files.Remove(fileToDelete);

        var result = await Context.SaveChangesAsync();

        return result == 1 ? Ok() : BadRequest("File could not be deleted, please try again alter");
    }

    public static void DeleteFileFromDrive(GudFile file, ILogger logger)
    {
        try
        {
            System.IO.File.Delete($"{ImagesPath}/{file.UniqueId}.{file.FileExtension}");
        }
        catch (Exception)
        {
            logger.LogError("The file {FileName} couldn't be deleted", file.UniqueId);
        }

        try
        {
            System.IO.File.Delete($"{ThumbnailsPath}/{file.UniqueId}");
        }
        catch (Exception)
        {
            logger.LogError("The thumbnail file {FileName} couldn't be deleted", file.UniqueId);
        }
    }
}