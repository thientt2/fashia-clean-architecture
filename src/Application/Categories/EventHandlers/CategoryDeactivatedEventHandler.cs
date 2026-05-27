using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;
using Fashia.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fashia.Application.Categories.EventHandlers;

public class CategoryDeactivatedEventHandler : INotificationHandler<CategoryDeactivatedEvent>
{
    private readonly IEmailQueue _emailQueue;
    private readonly ILogger<CategoryDeactivatedEventHandler> _logger;

    public CategoryDeactivatedEventHandler(
        IEmailQueue emailQueue,
        ILogger<CategoryDeactivatedEventHandler> logger)
    {
        _emailQueue = emailQueue;
        _logger = logger;
    }

    public async Task Handle(
        CategoryDeactivatedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Queueing email for deactivated category {CategoryId}",
            notification.Category.Id);

        var message = new EmailMessage(
            "tratatmatong@gmail.com",
            $"Ve viec thay doi trang thai cua danh muc: {notification.Category.Name}",
            $"""
            <h2>Danh muc {notification.Category.Name} da bi vo hieu hoa</h2>
            <p><strong>Danh muc:</strong> {notification.Category.Name} se khong con hoat dong. Cac ban co the cap nhat thong tin hoac lien he voi quan tri vien de biet them chi tiet.</p>
            """);

        await _emailQueue.QueueAsync(message, cancellationToken);
    }
}
