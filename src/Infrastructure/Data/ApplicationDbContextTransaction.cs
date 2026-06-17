using Fashia.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fashia.Infrastructure.Data;

public sealed class ApplicationDbContextTransaction : IApplicationDbContextTransaction
{
    private readonly IDbContextTransaction _transaction;

    public ApplicationDbContextTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return _transaction.CommitAsync(cancellationToken);
    }

    public Task RollbackAsync(CancellationToken cancellationToken)
    {
        return _transaction.RollbackAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _transaction.DisposeAsync();
    }
}
