using Fashia.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fashia.Application.Categories.EventHandlers;

public class CategoryActivatedEventHandler : INotificationHandler<CategoryActivatedEvent>
{
    private readonly ILogger<CategoryActivatedEventHandler> _logger;

    public CategoryActivatedEventHandler(
        ILogger<CategoryActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        CategoryActivatedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Category activated. CategoryId: {CategoryId}, Name: {CategoryName}, Slug: {CategorySlug}",
            notification.Category.Id,
            notification.Category.Name,
            notification.Category.Slug);

        // TODO: Send email later
        // await _emailSender.SendAsync(...);

        return Task.CompletedTask;
    }
}