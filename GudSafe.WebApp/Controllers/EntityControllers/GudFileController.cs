using AutoMapper;
using GudSafe.Data;
using GudSafe.Data.Entities;
using GudSafe.WebApp.Classes.Attributes;
using GudSafe.WebApp.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Net.Http.Headers;
using SkiaSharp;

namespace GudSafe.WebApp.Controllers.EntityControllers;

[Route("files")]
[Route("f")]
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

        if (!parts.Any())
        {
            return BadRequest("Couldn't parse filename and file extension");
        }

        GudFile? dbFile;

        try
        {
            if (Guid.TryParse(parts[0], out var guid))
                dbFile = await Context.Files.FirstOrDefaultAsync(x => x.UniqueId == guid);
            else
                dbFile = await Context.Files.FirstOrDefaultAsync(x => x.ShortUrl == parts[0]);
        }
        catch (Exception)
        {
            Logger.LogWarning("Couldn't find the file {name} via GUID nor via short URL!", name);

            return NotFound("Couldn't find the requested file");
        }

        if (dbFile == null)
            return NotFound();

        if (parts.Length != 2)
            return RedirectPermanent($"/f/{dbFile.UniqueId}.{dbFile.FileExtension}");

        if (dbFile.FileExtension != parts[1])
            return NotFound("Couldn't find the requested file");

        var path = Path.Combine(ImagesPath, $"{dbFile.UniqueId}.{dbFile.FileExtension}");

        try
        {
            var file = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            var contentDispositionHeader = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = dbFile.Name
            };

            HttpContext.Response.Headers.ContentDisposition = contentDispositionHeader.ToString();
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

        if (!parts.Any())
        {
            return BadRequest("Couldn't parse filename and file extension");
        }

        GudFile? dbFile;

        try
        {
            if (Guid.TryParse(parts[0], out var guid))
                dbFile = await Context.Files.FirstOrDefaultAsync(x => x.UniqueId == guid);
            else
                dbFile = await Context.Files.FirstOrDefaultAsync(x => x.ShortUrl == parts[0]);
        }
        catch (Exception)
        {
            Logger.LogWarning("Couldn't find the file {name} via GUID nor via short URL!", name);

            return NotFound("Couldn't find the requested file");
        }

        if (dbFile == null)
            return NotFound();

        if (parts.Length == 2 && dbFile.FileExtension != parts[1])
            return NotFound("Couldn't find the requested file");

        var path = Path.Combine(ThumbnailsPath, $"{dbFile.UniqueId}");

        try
        {
            var file = System.IO.File.Open(path, FileMode.Open);

            var contentDispositionHeader = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = dbFile.Name
            };

            HttpContext.Response.Headers.ContentDisposition = contentDispositionHeader.ToString();
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

        var newEntry = await UploadFile(file, user, Context);

        await GenerateThumbnail(newEntry.Entity);

        await _uploadHub.Clients.User(user.UniqueId.ToString()).SendAsync("RefreshFiles");

        var success = bool.TryParse(Environment.GetEnvironmentVariable("FORCE_HTTPS"), out var result);

        if (success && result)
        {
            return Ok(new
            {
                Url = $"https://{Request.Host}/f/{newEntry.Entity.ShortUrl}.{newEntry.Entity.FileExtension}",
                ThumbnailUrl =
                    $"https://{Request.Host}/f/{newEntry.Entity.ShortUrl}.{newEntry.Entity.FileExtension}/thumbnail"
            });
        }

        return Ok(new
        {
            Url = $"{Request.Scheme}://{Request.Host}/f/{newEntry.Entity.ShortUrl}.{newEntry.Entity.FileExtension}",
            ThumbnailUrl =
                $"{Request.Scheme}://{Request.Host}/f/{newEntry.Entity.ShortUrl}.{newEntry.Entity.FileExtension}/thumbnail"
        });
    }

    public static async Task<EntityEntry<GudFile>> UploadFile(IFormFile file, User user, GudSafeContext context)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(file.FileName, out var contentType);

        await using var stream = file.OpenReadStream();

        var newFile = new GudFile
        {
            Creator = user,
            FileExtension = Path.GetExtension(file.FileName)[1..],
            FileType = contentType ?? file.ContentType,
            Name = file.FileName
        };

        GenerateShortUrl(ref newFile);

        var newEntry = await context.Files.AddAsync(newFile);

        var imagePath = Path.Combine(ImagesPath, $"{newFile.UniqueId}.{newFile.FileExtension}");

        Directory.CreateDirectory(ThumbnailsPath);

        await using var imageFs = new FileStream(imagePath, FileMode.Create);
        await stream.CopyToAsync(imageFs);

        await context.SaveChangesAsync();

        return newEntry;
    }

    public static async Task UploadChunk(IFormFile file, GudFile gudFile, int curChunk, GudSafeContext context)
    {
        var imagePath = Path.Combine(ImagesPath, $"{gudFile.UniqueId}.{gudFile.FileExtension}");

        await using var stream = file.OpenReadStream();

        await using var imageFs = new FileStream(imagePath, FileMode.Append);

        await stream.CopyToAsync(imageFs);
    }

    public static async Task GenerateThumbnail(GudFile file)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(file.Name, out var contentType);

        var imagePath = Path.Combine(ImagesPath, $"{file.UniqueId}.{file.FileExtension}");
        var thumbnailPath = Path.Combine(ThumbnailsPath, file.UniqueId.ToString());

        if (contentType?.Contains("image") ?? false)
        {
            await using var stream = System.IO.File.OpenRead(imagePath);

            await ImageToThumbnail(stream, thumbnailPath, file);
        }
        else
        {
            await ExtensionToThumbnail(file, thumbnailPath);
        }
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

    public static void GenerateShortUrl(ref GudFile file)
    {
        // First we generate a short URL from our UniqueId
        var guidString = Convert.ToBase64String(file.UniqueId.ToByteArray());

        var shortUrl = guidString.Replace("=", "");
        shortUrl = shortUrl.Replace("+", "");
        shortUrl = shortUrl.Replace("/", "");
        shortUrl = shortUrl.Replace("\\", "");

        shortUrl = shortUrl[..10];

        using var context = new GudSafeContext();

        var isInDb = context.Files.Any(x => x.ShortUrl == shortUrl);

        // Check if for some reason the short URL is already in the Database
        // Then we generate short URLs until we find one that isn't used yet!
        while (isInDb)
        {
            var base64String = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            base64String = base64String.Replace("=", "");

            base64String = base64String.Replace("+", "");
            base64String = base64String.Replace("/", "");
            base64String = base64String.Replace("\\", "");

            shortUrl = base64String;

            isInDb = context.Files.Any(x => x.ShortUrl == shortUrl);
        }

        // Take the first 10 chars from the Base64 string
        file.ShortUrl = shortUrl[..10];
    }
}