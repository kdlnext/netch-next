using System.Web;
using Netch.Models;
using Netch.Utils;

namespace Netch.Servers;

public static class V2rayUtils
{
    public static string GetVShareLink(Server s, string type = "")
    {
        return s switch
        {
            VLESSServer vless => BuildVlessUri(vless),
            VMessServer vmess => BuildVmessUri(vmess),
            WireGuardServer wireGuard => BuildWireGuardUri(wireGuard),
            SSHServer ssh => BuildSshUri(ssh),
            _ => string.Empty
        };
    }

    public static IEnumerable<Server> ParseVUri(string text)
    {
        if (text.StartsWith("vless://", StringComparison.OrdinalIgnoreCase))
            return ParseVlessUri(text);

        if (text.StartsWith("vmess://", StringComparison.OrdinalIgnoreCase))
            return ParseVmessUri(text);

        if (text.StartsWith("wireguard://", StringComparison.OrdinalIgnoreCase))
            return ParseWireGuardUri(text);

        if (text.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
            return ParseSshUri(text);

        return Array.Empty<Server>();
    }

    private static string BuildVlessUri(VLESSServer server)
    {
        var query = new List<string>
        {
            $"encryption={EncodeQueryValue(server.EncryptMethod)}"
        };

        AddQueryIfNotEmpty(query, "security", server.TLSSecureType, "none");
        AddQueryIfNotEmpty(query, "type", server.TransferProtocol, "tcp");
        AddQueryIfNotEmpty(query, "host", server.Host);
        AddQueryIfNotEmpty(query, "path", server.Path);
        AddQueryIfNotEmpty(query, "sni", server.ServerName);

        if (server.TLSSecureType == "reality")
        {
            AddQueryIfNotEmpty(query, "pbk", server.PublicKey);
            AddQueryIfNotEmpty(query, "sid", server.ShortId);
            AddQueryIfNotEmpty(query, "spx", server.SpiderX);
        }

        var queryText = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
        return $"vless://{server.UserID}@{server.Hostname}:{server.Port}{queryText}#{EncodeFragment(server.Remark)}";
    }

    private static string BuildVmessUri(VMessServer server)
    {
        var query = new List<string>();

        AddQueryIfNotEmpty(query, "security", server.EncryptMethod, "auto");
        AddQueryIfNotEmpty(query, "alterId", server.AlterID == 0 ? null : server.AlterID.ToString());
        AddQueryIfNotEmpty(query, "type", server.TransferProtocol, "tcp");
        AddQueryIfNotEmpty(query, "headerType", server.FakeType, "none");
        AddQueryIfNotEmpty(query, "host", server.Host);
        AddQueryIfNotEmpty(query, "path", server.Path);
        AddQueryIfNotEmpty(query, "sni", server.ServerName);
        AddQueryIfNotEmpty(query, "tls", server.TLSSecureType == "none" ? null : server.TLSSecureType);

        var queryText = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
        return $"vmess://{server.UserID}@{server.Hostname}:{server.Port}{queryText}#{EncodeFragment(server.Remark)}";
    }

    private static string BuildWireGuardUri(WireGuardServer server)
    {
        var query = new List<string>();

        AddQueryIfNotEmpty(query, "address", server.LocalAddresses);
        AddQueryIfNotEmpty(query, "publickey", server.PeerPublicKey);
        AddQueryIfNotEmpty(query, "privatekey", server.PrivateKey);
        AddQueryIfNotEmpty(query, "presharedkey", server.PreSharedKey);
        AddQueryIfNotEmpty(query, "mtu", server.MTU.ToString());

        return $"wireguard://{server.Hostname}:{server.Port}?{string.Join("&", query)}#{EncodeFragment(server.Remark)}";
    }

    private static string BuildSshUri(SSHServer server)
    {
        var query = new List<string>();

        AddQueryIfNotEmpty(query, "private_key", server.PrivateKey);
        AddQueryIfNotEmpty(query, "public_key", server.PublicKey);

        var password = string.IsNullOrEmpty(server.Password) ? string.Empty : $":{HttpUtility.UrlEncode(server.Password)}";
        var queryText = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

        return $"ssh://{HttpUtility.UrlEncode(server.User)}{password}@{server.Hostname}:{server.Port}{queryText}#{EncodeFragment(server.Remark)}";
    }

    private static IEnumerable<Server> ParseVlessUri(string text)
    {
        var result = new List<Server>();

        try
        {
            var uri = new Uri(text);
            var server = new VLESSServer
            {
                UserID = Uri.UnescapeDataString(uri.UserInfo),
                Hostname = uri.Host,
                Port = (ushort)(uri.Port == -1 ? 443 : uri.Port),
                Remark = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'))
            };

            var queryParams = ShareLink.ParseParam(uri.Query.TrimStart('?'));

            if (queryParams.TryGetValue("security", out var security))
                server.TLSSecureType = Uri.UnescapeDataString(security);

            if (queryParams.TryGetValue("encryption", out var encryption))
                server.EncryptMethod = Uri.UnescapeDataString(encryption);

            if (queryParams.TryGetValue("type", out var type))
                server.TransferProtocol = Uri.UnescapeDataString(type);

            if (queryParams.TryGetValue("sni", out var sni))
                server.ServerName = Uri.UnescapeDataString(sni);

            if (queryParams.TryGetValue("pbk", out var pbk))
                server.PublicKey = Uri.UnescapeDataString(pbk);

            if (queryParams.TryGetValue("sid", out var sid))
                server.ShortId = Uri.UnescapeDataString(sid);

            if (queryParams.TryGetValue("spx", out var spx))
                server.SpiderX = Uri.UnescapeDataString(spx);
            else if (queryParams.TryGetValue("spiderX", out spx))
                server.SpiderX = Uri.UnescapeDataString(spx);

            if (queryParams.TryGetValue("path", out var path))
                server.Path = Uri.UnescapeDataString(path);

            if (queryParams.TryGetValue("host", out var host))
                server.Host = Uri.UnescapeDataString(host);

            result.Add(server);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Parse VLESS URI failed");
        }

        return result;
    }

    private static IEnumerable<Server> ParseVmessUri(string text)
    {
        var result = new List<Server>();

        try
        {
            var uri = new Uri(text);
            var server = new VMessServer
            {
                UserID = Uri.UnescapeDataString(uri.UserInfo),
                Hostname = uri.Host,
                Port = (ushort)(uri.Port == -1 ? 443 : uri.Port),
                Remark = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'))
            };

            var queryParams = ShareLink.ParseParam(uri.Query.TrimStart('?'));

            if (queryParams.TryGetValue("security", out var security))
                server.EncryptMethod = Uri.UnescapeDataString(security);

            if (queryParams.TryGetValue("alterId", out var alterId) && int.TryParse(alterId, out var aid))
                server.AlterID = aid;

            if (queryParams.TryGetValue("type", out var type))
                server.TransferProtocol = Uri.UnescapeDataString(type);

            if (queryParams.TryGetValue("headerType", out var headerType))
                server.FakeType = Uri.UnescapeDataString(headerType);

            if (queryParams.TryGetValue("host", out var host))
                server.Host = Uri.UnescapeDataString(host);

            if (queryParams.TryGetValue("path", out var path))
                server.Path = Uri.UnescapeDataString(path);

            if (queryParams.TryGetValue("sni", out var sni))
                server.ServerName = Uri.UnescapeDataString(sni);

            if (queryParams.TryGetValue("tls", out var tls))
                server.TLSSecureType = Uri.UnescapeDataString(tls);

            result.Add(server);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Parse VMess URI failed");
        }

        return result;
    }

    private static IEnumerable<Server> ParseWireGuardUri(string text)
    {
        var result = new List<Server>();

        try
        {
            var uri = new Uri(text);
            var server = new WireGuardServer
            {
                Hostname = uri.Host,
                Port = (ushort)(uri.Port == -1 ? 51820 : uri.Port),
                Remark = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'))
            };

            var queryParams = ShareLink.ParseParam(uri.Query.TrimStart('?'));

            if (queryParams.TryGetValue("address", out var address))
                server.LocalAddresses = Uri.UnescapeDataString(address);

            if (queryParams.TryGetValue("publickey", out var publicKey))
                server.PeerPublicKey = Uri.UnescapeDataString(publicKey);

            if (queryParams.TryGetValue("privatekey", out var privateKey))
                server.PrivateKey = Uri.UnescapeDataString(privateKey);

            if (queryParams.TryGetValue("presharedkey", out var preSharedKey))
                server.PreSharedKey = Uri.UnescapeDataString(preSharedKey);

            if (queryParams.TryGetValue("mtu", out var mtu) && int.TryParse(mtu, out var parsedMtu))
                server.MTU = parsedMtu;

            result.Add(server);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Parse WireGuard URI failed");
        }

        return result;
    }

    private static IEnumerable<Server> ParseSshUri(string text)
    {
        var result = new List<Server>();

        try
        {
            var uri = new Uri(text);
            var userInfoParts = uri.UserInfo.Split(':', 2);
            var server = new SSHServer
            {
                User = userInfoParts.Length > 0 ? HttpUtility.UrlDecode(userInfoParts[0]) ?? string.Empty : string.Empty,
                Password = userInfoParts.Length > 1 ? HttpUtility.UrlDecode(userInfoParts[1]) ?? string.Empty : string.Empty,
                Hostname = uri.Host,
                Port = (ushort)(uri.Port == -1 ? 22 : uri.Port),
                Remark = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'))
            };

            var queryParams = ShareLink.ParseParam(uri.Query.TrimStart('?'));

            if (queryParams.TryGetValue("private_key", out var privateKey))
                server.PrivateKey = Uri.UnescapeDataString(privateKey);

            if (queryParams.TryGetValue("public_key", out var publicKey))
                server.PublicKey = Uri.UnescapeDataString(publicKey);

            result.Add(server);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Parse SSH URI failed");
        }

        return result;
    }

    private static void AddQueryIfNotEmpty(ICollection<string> query, string key, string? value, string? skipValue = null)
    {
        if (string.IsNullOrEmpty(value) || string.Equals(value, skipValue, StringComparison.OrdinalIgnoreCase))
            return;

        query.Add($"{key}={EncodeQueryValue(value)}");
    }

    private static string EncodeQueryValue(string value)
    {
        return Uri.EscapeDataString(value);
    }

    private static string EncodeFragment(string value)
    {
        return Uri.EscapeDataString(value ?? string.Empty);
    }
}

public class V2rayNJObject
{
    public int v { get; set; }
    public string ps { get; set; } = "";
    public string add { get; set; } = "";
    public ushort port { get; set; }
    public string scy { get; set; } = "";
    public string id { get; set; } = "";
    public int aid { get; set; }
    public string net { get; set; } = "";
    public string type { get; set; } = "";
    public string host { get; set; } = "";
    public string path { get; set; } = "";
    public string tls { get; set; } = "";
    public string sni { get; set; } = "";
}
