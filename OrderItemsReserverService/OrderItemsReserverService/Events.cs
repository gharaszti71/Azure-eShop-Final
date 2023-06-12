using Microsoft.Extensions.Logging;

namespace OrderItemsReserverService
{
    internal static class Events
    {
        internal static EventId FallbackFailed = new EventId(1001, nameof(FallbackFailed));
        internal static EventId FallbackNeed = new EventId(1002, nameof(FallbackNeed));
    }
}
