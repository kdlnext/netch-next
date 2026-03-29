using System;
using System.Collections.Generic;
using System.Web;
using Netch.Interfaces;
using Netch.Models;

namespace Netch.Servers;

public class TUICUtil : IServerUtil
{
    public ushort Priority { get; } = 9;
    public string TypeName { get; } = "TUIC";
    public string FullName { get; } = "TUIC";
    public string ShortName { get; } = "TUIC";
    public string[] UriScheme { get; } = { "tuic" };
    public Type ServerType { get; } = typeof(TUICServer);

    public void Edit(Server s)
    {
        new TUICForm((TUICServer)s).ShowDialog();
    }

    public void Create()
    {
        new TUICForm().ShowDialog();
    }

    public string GetShareLink(Server s)
    {
        var server = (TUICServer)s;
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(server.SNI))
            query.Add($"sni={Uri.EscapeDataString(server.SNI)}");

        if (server.SkipCertVerify)
            query.Add("allow_insecure=1");

        if (!string.IsNullOrWhiteSpace(server.CongestionControl))
            query.Add($"congestion_control={Uri.EscapeDataString(server.CongestionControl)}");

        var queryText = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
        return $"tuic://{server.UUID}:{HttpUtility.UrlEncode(server.Password)}@{server.Hostname}:{server.Port}{queryText}#{Uri.EscapeDataString(server.Remark)}";
    }

    public IServerController GetController()
    {
        return new SingboxController();
    }

    public IEnumerable<Server> ParseUri(string text)
    {
        var result = new List<Server>();
        if (text.StartsWith("tuic://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(text);
                var server = new TUICServer
                {
                    Hostname = uri.Host,
                    Port = (ushort)(uri.Port == -1 ? 443 : uri.Port),
                    Remark = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'))
                };

                var userInfo = uri.UserInfo.Split(':');
                if (userInfo.Length >= 1) server.UUID = userInfo[0];
                if (userInfo.Length >= 2) server.Password = userInfo[1];

                var queryParams = Netch.Utils.ShareLink.ParseParam(uri.Query.TrimStart('?'));

                if (queryParams.TryGetValue("sni", out var sni))
                    server.SNI = sni;

                if (queryParams.TryGetValue("allow_insecure", out var insecure))
                    server.SkipCertVerify = insecure == "1";

                if (queryParams.TryGetValue("congestion_control", out var cc))
                    server.CongestionControl = cc;

                result.Add(server);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Parse TUIC URI failed");
            }
        }
        return result;
    }

    public bool CheckServer(Server s)
    {
        return true;
    }
}
