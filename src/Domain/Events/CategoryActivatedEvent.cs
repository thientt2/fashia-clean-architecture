namespace Fashia.Domain.Events;

public class CategoryActivatedEvent : BaseEvent
{
    public CategoryActivatedEvent(Category category)
    {
        Category = category;
    }

    public Category Category { get; }
}