namespace Fashia.Application.Common.Interfaces;

public interface IApplicationDbContextTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}
