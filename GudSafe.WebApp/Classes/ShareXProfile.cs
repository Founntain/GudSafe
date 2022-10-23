namespace GudSafe.WebApp.Classes;

public class ShareXProfile
{
    public string Name { get; init; }
    public string DestinationType { get; init; }
    public string RequestMethod { get; init; }
    public string RequestURL { get; init; }
    public string Body { get; init; }
    public Dictionary<string, object> Headers { get; init; }
    public string FileFormName { get; init; }
    public string URL { get; init; }
    public string ThumbnailURL { get; init; }
}