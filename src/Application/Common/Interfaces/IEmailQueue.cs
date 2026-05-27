using Fashia.Application.Common.Models;

namespace Fashia.Application.Common.Interfaces;

public interface IEmailQueue
{
    ValueTask QueueAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<EmailMessage> DequeueAllAsync(
        CancellationToken cancellationToken = default);
}
