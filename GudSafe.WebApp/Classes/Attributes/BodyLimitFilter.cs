using GudSafe.Data.Configuration;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GudSafe.WebApp.Classes.Attributes;

public class BodyLimitFilter : IAuthorizationFilter, IRequestSizePolicy, IRequestFormLimitsPolicy
{
    private ILogger _logger;
    private ConfigService _configuration;

    public BodyLimitFilter(ILoggerFactory loggerFactory, ConfigService configuration)
    {
        _logger = loggerFactory.CreateLogger<BodyLimitAttribute>();
        _configuration = configuration;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var effectivePolicy = context.FindEffectivePolicy<IRequestFormLimitsPolicy>();
        if (effectivePolicy != null && effectivePolicy != this)
        {
            _logger.LogError("{Type} is not {EffectivePolicyType}", GetType(), effectivePolicy.GetType());
            return;
        }

        var features = context.HttpContext.Features;
        var formFeature = features.Get<IFormFeature>();

        if (formFeature == null || formFeature.Form == null)
        {
            features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, new FormOptions()
            {
                MultipartBodyLengthLimit = _configuration.Container.MaxUploadSizeMb * 1024L * 1024L,
            }));
            _logger.LogInformation("Applied body length limit to form feature");
        }

        var maxBodyLength = features.Get<IHttpMaxRequestBodySizeFeature>();

        if (maxBodyLength == null)
        {
            _logger.LogError("Max body length feature not found");
        }
        else if (maxBodyLength.IsReadOnly)
        {
            _logger.LogError("Max body length feature is read only");
        }
        else
        {
            maxBodyLength.MaxRequestBodySize = _configuration.Container.MaxUploadSizeMb * 1024L * 1024L;
            _logger.LogInformation("Applied max request body size");
        }
    }
}