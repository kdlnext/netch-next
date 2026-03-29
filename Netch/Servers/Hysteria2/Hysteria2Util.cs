using System;
using System.Collections.Generic;
using Netch.Interfaces;
using Netch.Models;

namespace Netch.Servers;

public class Hysteria2Util : IServerUtil
{
    public ushort Priority { get; } = 8;
    public string TypeName { get; } = "Hysteria2";
    public string FullName { get; } = "Hysteria2";
    public string ShortName { get; } = "HY2";
    public string[] UriScheme { get; } = { "hysteria2", "hy2" };
    public Type ServerType { get; } = typeof(Hysteria2Server);

    public void Edit(Server s)
    {
        new Hysteria2Form((Hysteria2Server)s).ShowDialog();
    }

    public void Create()
    {
        new Hysteria2Form().ShowDialog();
    }

    public string GetShareLink(Server s)
    {
        var server = (Hysteria2Server)s;
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(server.SNI))
            query.Add($"sni={Uri.EscapeDataString(server.SNI)}");

        if (server.SkipCertVerify)
            query.Add("insecure=1");

        if (!string.IsNullOrWhiteSpace(server.Obfs))
            query.Add($"obfs={Uri.EscapeDataString(server.Obfs)}");

        if (!string.IsNullOrWhiteSpace(server.ObfsPassword))
            query.Add($"obfs-password={Uri.EscapeDataString(server.ObfsPassword)}");

        if (server.UpMbps > 0)
            query.Add($"upmbps={server.UpMbps}");

        if (server.DownMbps > 0)
            query.Add($"downmbps={server.DownMbps}");

        var queryText = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
        return $"hy2://{Uri.EscapeDataString(server.Password)}@{server.Hostname}:{server.Port}{queryText}#{Uri.EscapeDataString(server.Remark)}";
    }

    public IServerController GetController()
    {
        return new SingboxController();
    }

    public IEnumerable<Server> ParseUri(string text)
    {
        var result = new List<Server>();
        if (text.StartsWith("hy2://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("hysteria2://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(text);
                var server = new Hysteria2Server
                {
                    Password = uri.UserInfo,
                    Hostname = uri.Host,
                    Port = (ushort)(uri.Port == -1 ? 443 : uri.Port),
                    Remark = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'))
                };

                var queryParams = Netch.Utils.ShareLink.ParseParam(uri.Query.TrimStart('?'));

                if (queryParams.TryGetValue("sni", out var sni) || queryParams.TryGetValue("peer", out sni))
                    server.SNI = sni;

                if (queryParams.TryGetValue("insecure", out var insecure))
                    server.SkipCertVerify = insecure == "1";

                if (queryParams.TryGetValue("obfs", out var obfs))
                    server.Obfs = obfs;

                if (queryParams.TryGetValue("obfs-password", out var obfspw))
                    server.ObfsPassword = obfspw;

                if (queryParams.TryGetValue("upmbps", out var upMbps) && int.TryParse(upMbps, out var parsedUpMbps))
                    server.UpMbps = parsedUpMbps;

                if (queryParams.TryGetValue("downmbps", out var downMbps) && int.TryParse(downMbps, out var parsedDownMbps))
                    server.DownMbps = parsedDownMbps;

                result.Add(server);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Parse Hysteria2 URI failed");
            }
        }
        return result;
    }

    public bool CheckServer(Server s)
    {
        return true;
    }
}
