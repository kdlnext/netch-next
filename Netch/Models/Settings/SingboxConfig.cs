namespace Netch.Models;

public class SingboxConfig
{
    public bool SingboxNShareLink { get; set; } = false;
    public bool TCPFastOpen { get; set; } = false;
    public bool AllowInsecure { get; set; } = false;
}
