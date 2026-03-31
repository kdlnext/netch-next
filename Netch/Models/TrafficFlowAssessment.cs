namespace Netch.Models;

public class TrafficFlowAssessment
{
    public required TrafficFlowType FlowType { get; init; }

    public required string HijackDifficulty { get; init; }

    public required string TransparentProxyDifficulty { get; init; }

    public required bool CapturableByWinDivert { get; init; }

    public required string ProcessMappingReliability { get; init; }

    public required string SuitableForLocalProxyRedirection { get; init; }

    public required bool NeedsOriginalDestination { get; init; }

    public required string NatAndLatencySensitivity { get; init; }

    public required TrafficHandlingStrategy BestStrategy { get; init; }

    public required string KnownBoundariesAndFailureModes { get; init; }
}
