using System.IO;
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

    public static string GetLocalVersion()
    {
        var binPath = Path.Combine(Global.NetchDir, "bin", "sing-box.exe");
        if (!File.Exists(binPath)) return "0.0.0";
        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = binPath,
                    Arguments = "version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            var match = System.Text.RegularExpressions.Regex.Match(output, @"version\s+([0-9\.]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            
            var match2 = System.Text.RegularExpressions.Regex.Match(output, @"([0-9]+\.[0-9]+\.[0-9]+)");
            if (match2.Success)
            {
                return match2.Groups[1].Value;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get local sing-box version by running process");
        }

        try
        {
            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(binPath);
            if (!string.IsNullOrEmpty(versionInfo.ProductVersion))
            {
                return versionInfo.ProductVersion;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get local sing-box version from FileVersionInfo");
        }

        return "0.0.0";
    }

    public static bool IsNewVersionAvailable(string latestTag)
    {
        var localStr = GetLocalVersion();
        var latestStr = latestTag.TrimStart('v');
        if (Version.TryParse(localStr, out var localVer) && Version.TryParse(latestStr, out var latestVer))
        {
            return latestVer > localVer;
        }
        return latestStr != localStr;
    }

    public static async Task<bool> DownloadAndReplaceCoreAsync()
    {
        if (LatestRelease == null) return false;
        var url = GetLatestDownloadUrl();
        if (string.IsNullOrEmpty(url)) return false;

        var tempDir = Path.Combine(Global.NetchDir, "temp_singbox_extract");
        var tempZipPath = Path.Combine(Global.NetchDir, "temp_singbox.zip");
        var targetExePath = Path.Combine(Global.NetchDir, "bin", "sing-box.exe");

        try
        {
            if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

            Log.Information("Downloading sing-box update from {Url}", url);
            await WebUtil.DownloadFileAsync(url, tempZipPath);

            Log.Information("Extracting sing-box update zip");
            System.IO.Compression.ZipFile.ExtractToDirectory(tempZipPath, tempDir);

            var files = Directory.GetFiles(tempDir, "sing-box.exe", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                throw new FileNotFoundException("sing-box.exe not found in extracted zip archive.");
            }

            var extractedExe = files[0];

            var targetDir = Path.GetDirectoryName(targetExePath);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir!);
            }

            Log.Information("Replacing old sing-box.exe with new one");
            try
            {
                foreach (var proc in System.Diagnostics.Process.GetProcessesByName("sing-box"))
                {
                    proc.Kill();
                    proc.WaitForExit(2000);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to kill running sing-box processes before upgrade");
            }

            File.Copy(extractedExe, targetExePath, true);
            Log.Information("sing-box.exe has been updated successfully!");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update sing-box core");
            return false;
        }
        finally
        {
            try
            {
                if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to clean up temp update files/directories");
            }
        }
    }
}
