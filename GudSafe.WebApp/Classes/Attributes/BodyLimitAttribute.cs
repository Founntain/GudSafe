using Microsoft.AspNetCore.Mvc.Filters;

namespace GudSafe.WebApp.Classes.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class BodyLimitAttribute : Attribute, IFilterFactory, IOrderedFilter
{
    public bool IsReusable => true;
    public int Order => 900;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<BodyLimitFilter>();
        return service;
    }
}