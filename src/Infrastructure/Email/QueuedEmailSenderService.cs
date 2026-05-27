using Fashia.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fashia.Infrastructure.Email;

public sealed class QueuedEmailSenderService : BackgroundService
{
    private readonly IEmailQueue _emailQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<QueuedEmailSenderService> _logger;

    public QueuedEmailSenderService(
        IEmailQueue emailQueue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<QueuedEmailSenderService> logger)
    {
        _emailQueue = emailQueue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _emailQueue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                await emailSender.SendAsync(
                    message.To,
                    message.Subject,
                    message.HtmlBody,
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send queued email to {EmailTo}",
                    message.To);
            }
        }
    }
}
