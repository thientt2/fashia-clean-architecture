using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fashia.Application.Categories.EventHandlers;

public class CategoryDeactivatedEventHandler : INotificationHandler<CategoryDeactivatedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<CategoryDeactivatedEventHandler> _logger;

    public CategoryDeactivatedEventHandler(
        IEmailSender emailSender,
        ILogger<CategoryDeactivatedEventHandler> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Handle(
        CategoryDeactivatedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sending email for deactivated category {CategoryId}",
            notification.Category.Id);

        await _emailSender.SendAsync(
            "tratatmatong@gmail.com",
            $"Về việc thay đổi trạng thái của danh mục: {notification.Category.Name}",
            $"""
            <h2>Danh mục {notification.Category.Name} đã bị vô hiệu hóa</h2>
            <p><strong>Danh mục:</strong> {notification.Category.Name} sẽ không còn hoạt động. Các bạn có thể cập nhật thông tin hoặc liên hệ với quản trị viên để biết thêm chi tiết.</p>
            """,
            cancellationToken);
    }
}