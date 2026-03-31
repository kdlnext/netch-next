using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using Netch.Controllers;
using Netch.Interfaces;
using Netch.Models;
using Netch.Utils;

namespace Netch.Servers;

public class SingboxController : Guard, IServerController
{
    public SingboxController() : base("sing-box.exe")
    {
    }

    protected override IEnumerable<string> StartedKeywords => new[] { "started", "listening at" };

    protected override IEnumerable<string> FailedKeywords => new[] { "fatal", "error" };

    public override string Name => "sing-box";

    public ushort? Socks5LocalPort { get; set; }

    public string? LocalAddress { get; set; }

    public virtual async Task<Socks5Server> StartAsync(Server s)
    {
        LocalAddress = this.LocalAddress();
        Socks5LocalPort = this.Socks5LocalPort();

        var configPath = Path.Combine(Global.NetchDir, Constants.TempConfig);

        await using (var fileStream = new FileStream(configPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            var config = await SingboxConfigUtils.GenerateClientConfigAsync(s, LocalAddress, Socks5LocalPort.Value);
            await JsonSerializer.SerializeAsync(fileStream, config, Global.NewCustomJsonSerializerOptions());
        }

        try
        {
            Log.Information("Starting sing-box on configured local endpoint {Address}:{Port}", LocalAddress, Socks5LocalPort.Value);
            await StartGuardAsync($"run -c \"{configPath}\"");
            await EnsureListeningPortExistsAsync(Socks5LocalPort.Value);
            Log.Information("sing-box listening confirmed on configured local port {Port}", Socks5LocalPort.Value);
            return new Socks5Server(IPAddress.Loopback.ToString(), this.Socks5LocalPort(), s.Hostname);
        }
        catch (Exception e)
        {
            Log.Warning(e, "sing-box startup failed on configured local port {Port}", Socks5LocalPort.Value);
            await StopAsync();
            throw new MessageException($"sing-box 控制器启动失败: {e.Message}");
        }
    }

    private static async Task EnsureListeningPortExistsAsync(ushort port)
    {
        for (var i = 0; i < 20; i++)
        {
            await Task.Delay(100);
            if (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(ipEndPoint => ipEndPoint.Port == port))
                return;
        }

        throw new MessageException($"sing-box 本地入站端口未成功监听: {port}");
    }
}
