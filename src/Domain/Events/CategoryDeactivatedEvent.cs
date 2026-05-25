using Fashia.Domain.Events;
public class CategoryDeactivatedEvent : BaseEvent
{
    public CategoryDeactivatedEvent(Category category)
    {
        Category = category;
    }

    public Category Category { get; }
}