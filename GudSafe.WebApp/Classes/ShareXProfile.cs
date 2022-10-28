// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace GudSafe.WebApp.Classes;

public class ShareXProfile
{
    public string Name { get; init; } = string.Empty;
    public string DestinationType { get; init; } = string.Empty;
    public string RequestMethod { get; init; } = string.Empty;
    public string RequestUrl { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Dictionary<string, object>? Headers { get; init; }
    public string FileFormName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string ThumbnailUrl { get; init; } = string.Empty;
}