namespace Netch.Models;

public class RedirectorConfig
{
    public bool FilterTCP { get; set; } = true;

    public bool FilterUDP { get; set; } = true;

    public bool FilterDNS { get; set; } = true;

    // Keep launcher-style acceleration behavior closer to classic Netch:
    // when the user selects a launcher/app process, child processes are
    // captured by default unless explicitly disabled.
    public bool FilterParent { get; set; } = true;

    public bool HandleOnlyDNS { get; set; } = true;

    public bool DNSProxy { get; set; } = true;

    public string DNSHost { get; set; } = $"{Constants.DefaultPrimaryDNS}:53";

    public int ICMPDelay { get; set; } = 10;

    public bool FilterICMP { get; set; } = false;
}
