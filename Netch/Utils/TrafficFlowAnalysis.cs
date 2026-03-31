using Netch.Models;

namespace Netch.Utils;

public static class TrafficFlowAnalysis
{
    public static IReadOnlyList<TrafficFlowAssessment> Get2026WindowsBaseline()
    {
        return new[]
        {
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.TCP,
                HijackDifficulty = "低",
                TransparentProxyDifficulty = "低到中",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "高",
                SuitableForLocalProxyRedirection = "高",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "中",
                BestStrategy = TrafficHandlingStrategy.PreserveOriginalDestinationWithSideChannel,
                KnownBoundariesAndFailureModes = "最适合作为进程级劫持主战场；失败主要来自 PID 归属错误、回环、端口映射冲突或本地代理未就绪。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.UDP,
                HijackDifficulty = "中",
                TransparentProxyDifficulty = "高",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中",
                SuitableForLocalProxyRedirection = "中",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "高",
                BestStrategy = TrafficHandlingStrategy.RedirectConservatively,
                KnownBoundariesAndFailureModes = "Windows 侧可抓到包，但 UDP 缺少稳定连接语义；失败常见于会话漂移、端口复用、NAT 绑定变化和高频小包抖动。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.QUIC,
                HijackDifficulty = "中到高",
                TransparentProxyDifficulty = "高",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中",
                SuitableForLocalProxyRedirection = "中",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "很高",
                BestStrategy = TrafficHandlingStrategy.RedirectConservatively,
                KnownBoundariesAndFailureModes = "本质上属于 UDP 会话；连接迁移、0-RTT、路径变化和 HTTP/3 语义都会降低透明接管成功率。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.HTTP3,
                HijackDifficulty = "中到高",
                TransparentProxyDifficulty = "高",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中",
                SuitableForLocalProxyRedirection = "中",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "很高",
                BestStrategy = TrafficHandlingStrategy.RedirectConditionally,
                KnownBoundariesAndFailureModes = "HTTP/3 建立在 QUIC 之上；能代理不等于能保持所有原始行为，失败模式与 QUIC 基本一致。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.STUN,
                HijackDifficulty = "中",
                TransparentProxyDifficulty = "高",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中",
                SuitableForLocalProxyRedirection = "低到中",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "很高",
                BestStrategy = TrafficHandlingStrategy.RedirectConditionally,
                KnownBoundariesAndFailureModes = "STUN 对 NAT 行为极敏感；强行代理可能改变探测结果，适合分类后按模式选择代理、放行或记录。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.DTLS,
                HijackDifficulty = "中",
                TransparentProxyDifficulty = "高",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中",
                SuitableForLocalProxyRedirection = "中",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "高",
                BestStrategy = TrafficHandlingStrategy.RedirectConservatively,
                KnownBoundariesAndFailureModes = "可按 UDP 会话代理，但不能假设本地层能理解其全部握手和上层业务语义。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.DNS,
                HijackDifficulty = "低到中",
                TransparentProxyDifficulty = "中",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中到高",
                SuitableForLocalProxyRedirection = "高",
                NeedsOriginalDestination = false,
                NatAndLatencySensitivity = "中",
                BestStrategy = TrafficHandlingStrategy.RedirectToLocalProxy,
                KnownBoundariesAndFailureModes = "传统 DNS 最适合本地承接；但 DoH/DoQ/内嵌解析会降低显式 DNS 劫持价值。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.BitTorrentLike,
                HijackDifficulty = "中",
                TransparentProxyDifficulty = "高",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中",
                SuitableForLocalProxyRedirection = "低到中",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "高",
                BestStrategy = TrafficHandlingStrategy.ObserveAndLog,
                KnownBoundariesAndFailureModes = "特征流量往往连接多、端口杂、UDP 多、对 NAT 穿透敏感；适合观察与保守策略，不适合承诺透明加速。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.SRT,
                HijackDifficulty = "中",
                TransparentProxyDifficulty = "高",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中",
                SuitableForLocalProxyRedirection = "中",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "很高",
                BestStrategy = TrafficHandlingStrategy.RedirectConditionally,
                KnownBoundariesAndFailureModes = "SRT 对时延、抖动和重传行为敏感；可承接，但不应默认视为与普通 UDP 同等稳定。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.MASQUELike,
                HijackDifficulty = "高",
                TransparentProxyDifficulty = "很高",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中",
                SuitableForLocalProxyRedirection = "低到中",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "高",
                BestStrategy = TrafficHandlingStrategy.PreserveOriginalDestinationWithSideChannel,
                KnownBoundariesAndFailureModes = "MASQUE 类流量常把上层代理语义隐藏在 HTTP/3/QUIC 中；仅靠本地包层难以自然恢复原始目标。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.ECHImpactedTls,
                HijackDifficulty = "低到中",
                TransparentProxyDifficulty = "中",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "高",
                SuitableForLocalProxyRedirection = "高",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "中",
                BestStrategy = TrafficHandlingStrategy.PreserveOriginalDestinationWithSideChannel,
                KnownBoundariesAndFailureModes = "ECH 本身不阻止抓包，但会削弱基于 TLS 明文握手的识别能力，因此不能依赖 SNI 侧推业务流量。"
            },
            new TrafficFlowAssessment
            {
                FlowType = TrafficFlowType.UnknownUDP,
                HijackDifficulty = "高",
                TransparentProxyDifficulty = "很高",
                CapturableByWinDivert = true,
                ProcessMappingReliability = "中到低",
                SuitableForLocalProxyRedirection = "低到中",
                NeedsOriginalDestination = true,
                NatAndLatencySensitivity = "很高",
                BestStrategy = TrafficHandlingStrategy.RedirectConservatively,
                KnownBoundariesAndFailureModes = "最需要保守处理的类别；若强行代理，常见失败为静默黑洞、NAT 失配、时延抖动放大和应用层超时。"
            }
        };
    }

    public static string GetProcessHijackStartupSummary()
    {
        var assessments = Get2026WindowsBaseline().ToDictionary(a => a.FlowType);

        return string.Join("; ", new[]
        {
            $"TCP={assessments[TrafficFlowType.TCP].BestStrategy}",
            $"UDP={assessments[TrafficFlowType.UDP].BestStrategy}",
            $"QUIC={assessments[TrafficFlowType.QUIC].BestStrategy}",
            $"STUN={assessments[TrafficFlowType.STUN].BestStrategy}",
            $"UnknownUDP={assessments[TrafficFlowType.UnknownUDP].BestStrategy}"
        });
    }
}
