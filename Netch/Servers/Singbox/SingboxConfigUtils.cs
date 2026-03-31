using Netch.Models;
using Netch.Utils;

namespace Netch.Servers;

public static class SingboxConfigUtils
{
    public static async Task<object> GenerateClientConfigAsync(Server server, string localAddress, ushort localPort)
    {
        var inbound = new
        {
            type = "mixed",
            tag = "mixed-in",
            listen = localAddress,
            listen_port = localPort
        };

        var outbound = await TryGetOutboundAsync(server);

        return new
        {
            log = new { level = "info", timestamp = true },
            inbounds = new[] { inbound },
            outbounds = new object[]
            {
                outbound,
                new { type = "direct", tag = "direct" },
                new { type = "block", tag = "block" }
            },
            route = new
            {
                auto_detect_interface = true,
                final = "proxy"
            },
            dns = new
            {
                strategy = "prefer_ipv4",
                independent_cache = true
            }
        };
    }

    private static async Task<object> TryGetOutboundAsync(Server server)
    {
        var address = await server.AutoResolveHostnameAsync();
        
        switch (server)
        {
            case ShadowsocksServer ss:
                return new
                {
                    type = "shadowsocks",
                    tag = "proxy",
                    server = address,
                    server_port = server.Port,
                    method = ss.EncryptMethod,
                    password = ss.Password,
                    plugin = ss.HasPlugin() ? ss.Plugin : null,
                    plugin_opts = ss.HasPlugin() ? ss.PluginOption : null
                };
            case ShadowsocksRServer ssr:
                throw new MessageException("sing-box 不支持 ShadowsocksR 协议");
            case Hysteria2Server hy2:
                return new
                {
                    type = "hysteria2",
                    tag = "proxy",
                    server = address,
                    server_port = server.Port,
                    password = hy2.Password,
                    up_mbps = hy2.UpMbps > 0 ? hy2.UpMbps : (int?)null,
                    down_mbps = hy2.DownMbps > 0 ? hy2.DownMbps : (int?)null,
                    obfs = string.IsNullOrEmpty(hy2.Obfs) ? null : new {
                        type = hy2.Obfs,
                        password = hy2.ObfsPassword
                    },
                    tls = new {
                        enabled = true,
                        server_name = hy2.SNI,
                        insecure = hy2.SkipCertVerify
                    }
                };
            case TUICServer tuic:
                return new
                {
                    type = "tuic",
                    tag = "proxy",
                    server = address,
                    server_port = server.Port,
                    uuid = tuic.UUID,
                    password = tuic.Password,
                    congestion_control = tuic.CongestionControl,
                    tls = new {
                        enabled = true,
                        server_name = tuic.SNI,
                        insecure = tuic.SkipCertVerify
                    }
                };
            case VLESSServer vless:
                object? tlsVless = null;
                if (vless.TLSSecureType == "reality")
                {
                    tlsVless = new {
                        enabled = true,
                        server_name = GetServerName(vless.ServerName, vless.Host),
                        insecure = Global.Settings.SingboxConfig.AllowInsecure,
                        utls = new {
                            enabled = true,
                            fingerprint = "chrome"
                        },
                        reality = new {
                            enabled = true,
                            public_key = vless.PublicKey,
                            short_id = vless.ShortId
                        }
                    };
                }
                else if (vless.TLSSecureType != "none")
                {
                    tlsVless = new { enabled = true, server_name = GetServerName(vless.ServerName, vless.Host), insecure = Global.Settings.SingboxConfig.AllowInsecure };
                }
                
                return new
                {
                    type = "vless",
                    tag = "proxy",
                    server = address,
                    server_port = server.Port,
                    uuid = vless.UserID,
                    flow = (vless.TLSSecureType == "xtls" || vless.TLSSecureType == "reality") ? "xtls-rprx-vision" : null,
                    tls = tlsVless,
                    transport = GetTransport(vless.TransferProtocol, vless.Host, vless.Path)
                };
            case VMessServer vmess:
                var tlsVmess = vmess.TLSSecureType != "none" ? new { enabled = true, server_name = GetServerName(vmess.ServerName, vmess.Host), insecure = Global.Settings.SingboxConfig.AllowInsecure } : null;
                return new
                {
                    type = "vmess",
                    tag = "proxy",
                    server = address,
                    server_port = server.Port,
                    uuid = vmess.UserID,
                    security = vmess.EncryptMethod == "auto" ? "auto" : vmess.EncryptMethod,
                    alter_id = vmess.AlterID,
                    tls = tlsVmess,
                    transport = GetTransport(vmess.TransferProtocol, vmess.Host, vmess.Path)
                };
            case TrojanServer trojan:
                return new
                {
                    type = "trojan",
                    tag = "proxy",
                    server = address,
                    server_port = server.Port,
                    password = trojan.Password,
                    tls = new { enabled = true, server_name = GetServerName(trojan.Host, null), insecure = Global.Settings.SingboxConfig.AllowInsecure }
                };
            case WireGuardServer wg:
                return new
                {
                    type = "wireguard",
                    tag = "proxy",
                    server = address,
                    server_port = server.Port,
                    local_address = wg.LocalAddresses?.Split(',') ?? Array.Empty<string>(),
                    private_key = wg.PrivateKey,
                    peer_public_key = wg.PeerPublicKey,
                    pre_shared_key = wg.PreSharedKey,
                    mtu = wg.MTU
                };
            case SSHServer ssh:
                return new
                {
                    type = "ssh",
                    tag = "proxy",
                    server = address,
                    server_port = server.Port,
                    user = ssh.User,
                    password = ssh.Password,
                    private_key = ssh.PrivateKey,
                };
            case Socks5Server socks5:
                return new 
                {
                    type = "socks",
                    tag = "proxy",
                    server = address,
                    server_port = server.Port,
                    username = socks5.Username,
                    password = socks5.Password
                };
            default:
                throw new MessageException($"Unsupported server type {server.Type} for sing-box");
        }
    }

    private static string GetServerName(string? serverName, string? host)
    {
        if (!string.IsNullOrEmpty(serverName)) return serverName.Split(',')[0];
        if (!string.IsNullOrEmpty(host)) return host.Split(',')[0];
        return "";
    }

    private static object? GetTransport(string protocol, string? host, string? path)
    {
        if (string.IsNullOrEmpty(protocol) || protocol == "tcp") return null;
        
        switch (protocol)
        {
            case "ws":
                return new { type = "ws", path = path ?? "/", headers = new { Host = host?.Split(',')[0] } };
            case "grpc":
                return new { type = "grpc", service_name = path };
            case "quic":
                return new { type = "quic" };
            default:
                return null;
        }
    }
}
