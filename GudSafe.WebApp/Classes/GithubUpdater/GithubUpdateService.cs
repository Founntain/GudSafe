namespace GudSafe.WebApp.Classes.GithubUpdater;

public class GithubUpdateService
{
    public UpdateResponse LatestResponse { get; private set; }

    private readonly ILogger<GithubUpdateService> _logger;

    public GithubUpdateService(ILogger<GithubUpdateService> logger)
    {
        _logger = logger;
        
        LatestResponse = GitHub.CheckForUpdates(_logger).GetAwaiter().GetResult();

        _ = new Timer(CheckForUpdates, null, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
    }

    private async void CheckForUpdates(object? state)
    {
        LatestResponse = await GitHub.CheckForUpdates(_logger);
    }
}