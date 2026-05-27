using System.Threading.Channels;
using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;

namespace Fashia.Infrastructure.Email;

public sealed class ChannelEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _queue = Channel.CreateBounded<EmailMessage>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask QueueAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(message, cancellationToken);
    }

    public IAsyncEnumerable<EmailMessage> DequeueAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _queue.Reader.ReadAllAsync(cancellationToken);
    }
}
