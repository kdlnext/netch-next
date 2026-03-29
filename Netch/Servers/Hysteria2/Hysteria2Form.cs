using Netch.Forms;
using System.Collections.Generic;

namespace Netch.Servers;

[Fody.ConfigureAwait(true)]
public class Hysteria2Form : ServerForm
{
    public Hysteria2Form(Hysteria2Server? server = default)
    {
        server ??= new Hysteria2Server();
        Server = server;
        CreateTextBox("Password", "Password", s => true, s => server.Password = s, server.Password);
        CreateTextBox("SNI", "SNI", s => true, s => server.SNI = s, server.SNI);
        CreateCheckBox("SkipCertVerify", "Skip Cert Verify", b => server.SkipCertVerify = b, server.SkipCertVerify);
        CreateTextBox("UpMbps", "Up Mbps (0=Auto)", s => int.TryParse(s, out _), s => server.UpMbps = int.Parse(s), server.UpMbps.ToString(), 76);
        CreateTextBox("DownMbps", "Down Mbps (0=Auto)", s => int.TryParse(s, out _), s => server.DownMbps = int.Parse(s), server.DownMbps.ToString(), 76);
        CreateComboBox("Obfs", "Obfs", new List<string> { "", "salamander" }, s => server.Obfs = s, server.Obfs);
        CreateTextBox("ObfsPassword", "Obfs Password", s => true, s => server.ObfsPassword = s, server.ObfsPassword);
    }

    protected override string TypeName { get; } = "Hysteria2";
}
