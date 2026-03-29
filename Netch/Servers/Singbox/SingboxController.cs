using System.Net;
using System.Text.Json;
using Netch.Controllers;
using Netch.Interfaces;
using Netch.Models;

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
        var configPath = Path.Combine(Global.NetchDir, Constants.TempConfig);

        await using (var fileStream = new FileStream(configPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            var config = await SingboxConfigUtils.GenerateClientConfigAsync(s);
            await JsonSerializer.SerializeAsync(fileStream, config, Global.NewCustomJsonSerializerOptions());
        }

        // sing-box uses standard arg run -c <config_path>
        await StartGuardAsync($"run -c \"{configPath}\"");
        return new Socks5Server(IPAddress.Loopback.ToString(), this.Socks5LocalPort(), s.Hostname);
    }
}
