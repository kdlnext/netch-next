using Netch.Forms;
using System.Collections.Generic;

namespace Netch.Servers;

[Fody.ConfigureAwait(true)]
public class TUICForm : ServerForm
{
    public TUICForm(TUICServer? server = default)
    {
        server ??= new TUICServer();
        Server = server;
        CreateTextBox("UUID", "UUID", s => true, s => server.UUID = s, server.UUID);
        CreateTextBox("Password", "Password", s => true, s => server.Password = s, server.Password);
        CreateTextBox("SNI", "SNI", s => true, s => server.SNI = s, server.SNI);
        CreateCheckBox("SkipCertVerify", "Skip Cert Verify", b => server.SkipCertVerify = b, server.SkipCertVerify);
        CreateComboBox("CongestionControl", "Congestion", new List<string> { "bbr", "cubic", "new_reno" }, s => server.CongestionControl = s, server.CongestionControl);
    }

    protected override string TypeName { get; } = "TUIC";
}
