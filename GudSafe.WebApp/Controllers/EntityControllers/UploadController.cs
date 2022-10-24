using AutoMapper;
using GudSafe.Data;

namespace GudSafe.WebApp.Controllers.EntityControllers;

public class UploadController : BaseController<UploadController>
{
    public UploadController(GudSafeContext context, IMapper mapper, ILogger<UploadController> logger) : base(context, mapper, logger)
    {
    }

}