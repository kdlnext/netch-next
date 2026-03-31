namespace Netch.Models;

public enum TrafficHandlingStrategy
{
    RedirectToLocalProxy,
    RedirectConservatively,
    RedirectConditionally,
    PreserveOriginalDestinationWithSideChannel,
    BypassByDefault,
    ObserveAndLog
}
