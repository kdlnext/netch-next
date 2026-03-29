using System.Net;
using System.Text.Json;
using Netch.Models.GitHubRelease;
using Netch.Utils;

namespace Netch.Controllers;

public static class SingboxUpdateChecker
{
    public const string Owner = @"sagernet";
    public const string Repo = @"sing-box";

    public static Release? LatestRelease { get; private set; }

    public static event EventHandler? NewVersionFound;
    public static event EventHandler? NewVersionFoundFailed;
    public static event EventHandler? NewVersionNotFound;

    public static async Task CheckAsync()
    {
        try
        {
            var updater = new GitHubRelease(Owner, Repo);
            var url = updater.AllReleaseUrl;

            var (_, json) = await WebUtil.DownloadStringAsync(WebUtil.CreateRequest(url));

            var releases = JsonSerializer.Deserialize<List<Release>>(json)!;
            LatestRelease = releases.FirstOrDefault(r => !r.prerelease);
            
            if (LatestRelease == null) 
            {
                NewVersionNotFound?.Invoke(null, EventArgs.Empty);
                return;
            }

            Log.Information("Github latest sing-box release: {Version}", LatestRelease.tag_name);
            NewVersionFound?.Invoke(null, EventArgs.Empty);
        }
        catch (Exception e)
        {
            Log.Error(e, "Get sing-box releases error");
            NewVersionFoundFailed?.Invoke(null, EventArgs.Empty);
        }
    }

    public static string? GetLatestDownloadUrl()
    {
        if (LatestRelease == null) return null;
        var version = LatestRelease.tag_name.TrimStart('v');
        return $"https://github.com/sagernet/sing-box/releases/download/{LatestRelease.tag_name}/sing-box-{version}-windows-amd64.zip";
    }
}
