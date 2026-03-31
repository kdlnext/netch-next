namespace Netch.Models;

public enum TrafficFlowType
{
    TCP,
    UDP,
    QUIC,
    HTTP3,
    STUN,
    DTLS,
    DNS,
    BitTorrentLike,
    SRT,
    MASQUELike,
    ECHImpactedTls,
    UnknownUDP
}
