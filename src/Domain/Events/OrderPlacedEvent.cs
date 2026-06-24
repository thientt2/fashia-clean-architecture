namespace Fashia.Domain.Events;

public sealed class OrderPlacedEvent : BaseEvent
{
    public OrderPlacedEvent(Order order)
    {
        Order = order;
    }

    public Order Order { get; }
}
