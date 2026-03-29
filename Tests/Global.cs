using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netch.Servers;
using Netch.Utils;
using System.Linq;

namespace Tests;

[TestClass]
public class ShareLinkTests
{
    [TestMethod]
    public void VlessShareLinkRoundTripPreservesRealityFields()
    {
        var original = new VLESSServer
        {
            Hostname = "edge.example.com",
            Port = 443,
            UserID = "11111111-1111-1111-1111-111111111111",
            Remark = "reality node",
            TLSSecureType = "reality",
            TransferProtocol = "ws",
            Host = "cdn.example.com",
            Path = "/ws",
            ServerName = "sni.example.com",
            PublicKey = "pubkey",
            ShortId = "abcd",
            SpiderX = "/landing"
        };

        var parsed = AssertSingle<VLESSServer>(V2rayUtils.ParseVUri(V2rayUtils.GetVShareLink(original)));

        Assert.AreEqual(original.Hostname, parsed.Hostname);
        Assert.AreEqual(original.Port, parsed.Port);
        Assert.AreEqual(original.UserID, parsed.UserID);
        Assert.AreEqual(original.TLSSecureType, parsed.TLSSecureType);
        Assert.AreEqual(original.TransferProtocol, parsed.TransferProtocol);
        Assert.AreEqual(original.Host, parsed.Host);
        Assert.AreEqual(original.Path, parsed.Path);
        Assert.AreEqual(original.ServerName, parsed.ServerName);
        Assert.AreEqual(original.PublicKey, parsed.PublicKey);
        Assert.AreEqual(original.ShortId, parsed.ShortId);
        Assert.AreEqual(original.SpiderX, parsed.SpiderX);
    }

    [TestMethod]
    public void VmessShareLinkRoundTripPreservesTransportFields()
    {
        var original = new VMessServer
        {
            Hostname = "vmess.example.com",
            Port = 8443,
            UserID = "22222222-2222-2222-2222-222222222222",
            Remark = "vmess node",
            EncryptMethod = "auto",
            AlterID = 8,
            TransferProtocol = "ws",
            FakeType = "none",
            Host = "ws.example.com",
            Path = "/vmess",
            ServerName = "tls.example.com",
            TLSSecureType = "tls"
        };

        var parsed = AssertSingle<VMessServer>(V2rayUtils.ParseVUri(V2rayUtils.GetVShareLink(original)));

        Assert.AreEqual(original.Hostname, parsed.Hostname);
        Assert.AreEqual(original.Port, parsed.Port);
        Assert.AreEqual(original.UserID, parsed.UserID);
        Assert.AreEqual(original.AlterID, parsed.AlterID);
        Assert.AreEqual(original.TransferProtocol, parsed.TransferProtocol);
        Assert.AreEqual(original.Host, parsed.Host);
        Assert.AreEqual(original.Path, parsed.Path);
        Assert.AreEqual(original.ServerName, parsed.ServerName);
        Assert.AreEqual(original.TLSSecureType, parsed.TLSSecureType);
    }

    [TestMethod]
    public void WireGuardShareLinkRoundTripPreservesKeys()
    {
        var original = new WireGuardServer
        {
            Hostname = "wg.example.com",
            Port = 51820,
            Remark = "wg node",
            LocalAddresses = "172.16.0.2/32,fd00::2/128",
            PeerPublicKey = "peer-key",
            PrivateKey = "private-key",
            PreSharedKey = "psk",
            MTU = 1280
        };

        var parsed = AssertSingle<WireGuardServer>(V2rayUtils.ParseVUri(V2rayUtils.GetVShareLink(original)));

        Assert.AreEqual(original.LocalAddresses, parsed.LocalAddresses);
        Assert.AreEqual(original.PeerPublicKey, parsed.PeerPublicKey);
        Assert.AreEqual(original.PrivateKey, parsed.PrivateKey);
        Assert.AreEqual(original.PreSharedKey, parsed.PreSharedKey);
        Assert.AreEqual(original.MTU, parsed.MTU);
    }

    [TestMethod]
    public void SshShareLinkRoundTripPreservesCredentials()
    {
        var original = new SSHServer
        {
            Hostname = "ssh.example.com",
            Port = 22,
            Remark = "ssh node",
            User = "root",
            Password = "pa:ss",
            PrivateKey = "private",
            PublicKey = "public"
        };

        var parsed = AssertSingle<SSHServer>(V2rayUtils.ParseVUri(V2rayUtils.GetVShareLink(original)));

        Assert.AreEqual(original.User, parsed.User);
        Assert.AreEqual(original.Password, parsed.Password);
        Assert.AreEqual(original.PrivateKey, parsed.PrivateKey);
        Assert.AreEqual(original.PublicKey, parsed.PublicKey);
    }

    [TestMethod]
    public void TuicShareLinkCanBeParsedBack()
    {
        var util = new TUICUtil();
        var original = new TUICServer
        {
            Hostname = "tuic.example.com",
            Port = 443,
            Remark = "tuic node",
            UUID = "33333333-3333-3333-3333-333333333333",
            Password = "secret",
            SNI = "tuic-sni.example.com",
            SkipCertVerify = true,
            CongestionControl = "bbr"
        };

        var parsed = AssertSingle<TUICServer>(util.ParseUri(util.GetShareLink(original)));

        Assert.AreEqual(original.UUID, parsed.UUID);
        Assert.AreEqual(original.Password, parsed.Password);
        Assert.AreEqual(original.SNI, parsed.SNI);
        Assert.AreEqual(original.SkipCertVerify, parsed.SkipCertVerify);
        Assert.AreEqual(original.CongestionControl, parsed.CongestionControl);
    }

    [TestMethod]
    public void Hysteria2ShareLinkCanBeParsedBack()
    {
        var util = new Hysteria2Util();
        var original = new Hysteria2Server
        {
            Hostname = "hy2.example.com",
            Port = 443,
            Remark = "hy2 node",
            Password = "secret",
            SNI = "hy2-sni.example.com",
            SkipCertVerify = true,
            Obfs = "salamander",
            ObfsPassword = "mask",
            UpMbps = 50,
            DownMbps = 100
        };

        var parsed = AssertSingle<Hysteria2Server>(util.ParseUri(util.GetShareLink(original)));

        Assert.AreEqual(original.Password, parsed.Password);
        Assert.AreEqual(original.SNI, parsed.SNI);
        Assert.AreEqual(original.SkipCertVerify, parsed.SkipCertVerify);
        Assert.AreEqual(original.Obfs, parsed.Obfs);
        Assert.AreEqual(original.ObfsPassword, parsed.ObfsPassword);
        Assert.AreEqual(original.UpMbps, parsed.UpMbps);
        Assert.AreEqual(original.DownMbps, parsed.DownMbps);
    }

    [TestMethod]
    public void ShareLinkParseTextUnderstandsGeneratedLinks()
    {
        var link = V2rayUtils.GetVShareLink(new VLESSServer
        {
            Hostname = "sub.example.com",
            Port = 443,
            UserID = "44444444-4444-4444-4444-444444444444",
            Remark = "sub node"
        });

        var parsed = AssertSingle<VLESSServer>(ShareLink.ParseText(link));
        Assert.AreEqual("sub.example.com", parsed.Hostname);
        Assert.AreEqual("sub node", parsed.Remark);
    }

    private static T AssertSingle<T>(IEnumerable<Netch.Models.Server> servers) where T : Netch.Models.Server
    {
        var list = servers.ToList();
        Assert.AreEqual(1, list.Count);
        Assert.IsInstanceOfType(list[0], typeof(T));
        return (T)list[0];
    }
}
