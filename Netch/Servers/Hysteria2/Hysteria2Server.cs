using Netch.Models;

namespace Netch.Servers;

public class Hysteria2Server : Server
{
    public override string Type { get; } = "Hysteria2";

    public override string MaskedData()
    {
        return $"{Type}";
    }

    /// <summary>
    ///     密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     SNI
    /// </summary>
    public string SNI { get; set; } = string.Empty;

    /// <summary>
    ///     跳过证书验证
    /// </summary>
    public bool SkipCertVerify { get; set; }
    
    /// <summary>
    ///     上传速度 (Mbps)
    /// </summary>
    public int UpMbps { get; set; }
    
    /// <summary>
    ///     下载速度 (Mbps)
    /// </summary>
    public int DownMbps { get; set; }
    
    /// <summary>
    ///     混淆 (salamander 等)
    /// </summary>
    public string Obfs { get; set; } = string.Empty;

    /// <summary>
    ///     混淆密码
    /// </summary>
    public string ObfsPassword { get; set; } = string.Empty;
}
