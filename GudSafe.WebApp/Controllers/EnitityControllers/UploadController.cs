using System.Net.Http.Headers;
using AutoMapper;
using GudSafe.Data;
using Microsoft.AspNetCore.Mvc;

namespace GudSafe.WebApp.Controllers;

public class UploadController : BaseController<UploadController>
{
    public UploadController(GudSafeContext context, IMapper mapper, ILogger<UploadController> logger) : base(context, mapper, logger)
    {
    }

}