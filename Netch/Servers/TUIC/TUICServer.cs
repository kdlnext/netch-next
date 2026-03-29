using Netch.Models;

namespace Netch.Servers;

public class TUICServer : Server
{
    public override string Type { get; } = "TUIC";

    public override string MaskedData()
    {
        return $"{Type}";
    }

    public string UUID { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SNI { get; set; } = string.Empty;

    public bool SkipCertVerify { get; set; }

    public string CongestionControl { get; set; } = "bbr";
}
