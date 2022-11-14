using System.Diagnostics;
using System.Reflection;
using Octokit;

namespace GudSafe.WebApp.Classes.GithubUpdater;

/// <summary>
/// A static class to help us access various GitHub Repository data
/// Also it helps us provide updates for the osu!player and check if updates are available by checking our
/// GitHub-Repository releases
/// </summary>
public static class GitHub
{
    /// <summary>
    /// Checks if an older version is running and if so it will notify the user.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> to be used to log events with</param>
    /// <param name="releaseChannel">The release channel to be used</param>
    /// <returns>a UpdateResponse object</returns>
    public static async Task<UpdateResponse> CheckForUpdates(ILogger? logger, ReleaseChannels releaseChannel = ReleaseChannels.Stable)
    {
        try
        {
            var currentVersion = Assembly.GetEntryAssembly()!.ToVersionString();

            var release = await GetLatestRelease(releaseChannel);

            if (release == default)
                return new UpdateResponse
                {
                    IsNewVersionAvailable = false
                };

            if (currentVersion != release.TagName)
                return new UpdateResponse
                {
                    IsNewVersionAvailable = true,
                    HtmlUrl = release.HtmlUrl,
                    IsPrerelease = releaseChannel == ReleaseChannels.PreReleases,
                    Version = release.TagName,
                    ReleaseDate = release.CreatedAt,
                };

            return new UpdateResponse
            {
                IsNewVersionAvailable = false
            };
        }
        catch (RateLimitExceededException ex)
        {
            logger?.LogWarning("Can't check for updates, rate github limit exceeded - {Message}", ex.Message);

            return new UpdateResponse
            {
                IsNewVersionAvailable = false
            };
        }
    }

    /// <summary>
    /// Gets the latest release from GitHub
    /// </summary>
    /// <param name="releaseChannel">The release channel to be used</param>
    /// <returns>a GitHub release</returns>
    public static async Task<Release?> GetLatestRelease(ReleaseChannels releaseChannel = ReleaseChannels.Stable)
    {
        var github = new GitHubClient(new ProductHeaderValue("GudSafe"));

        var releases = await github.Repository.Release.GetAll("Founntain", "gudsafe");

        var includePreReleases = releaseChannel == ReleaseChannels.PreReleases;

        Release? latestRelease = null;

        foreach (var release in releases.OrderBy(x => x.CreatedAt))
        {
            if (release.Prerelease && !includePreReleases) continue;

            latestRelease = release;
        }

        return latestRelease;
    }
}