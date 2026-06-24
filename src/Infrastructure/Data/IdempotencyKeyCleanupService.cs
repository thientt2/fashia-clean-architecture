using Fashia.Application.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fashia.Infrastructure.Data;

public sealed class IdempotencyKeyCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly IdempotencyOptions _options;
    private readonly ILogger<IdempotencyKeyCleanupService> _logger;

    public IdempotencyKeyCleanupService(
        IServiceScopeFactory serviceScopeFactory,
        TimeProvider timeProvider,
        IOptions<IdempotencyOptions> options,
        ILogger<IdempotencyKeyCleanupService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.CleanupEnabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var deleted = await DeleteExpiredKeysAsync(
                    context,
                    _timeProvider.GetUtcNow().UtcDateTime,
                    stoppingToken);

                if (deleted > 0)
                {
                    _logger.LogInformation(
                        "Deleted {DeletedCount} expired idempotency key records.",
                        deleted);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean expired idempotency keys.");
            }

            await Task.Delay(_options.EffectiveCleanupInterval, _timeProvider, stoppingToken);
        }
    }

    public static async Task<int> DeleteExpiredKeysAsync(
        ApplicationDbContext context,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var expiredKeys = await context
            .IdempotencyKeys.Where(x => x.ExpiresAt <= utcNow)
            .ToListAsync(cancellationToken);

        context.IdempotencyKeys.RemoveRange(expiredKeys);

        return await context.SaveChangesAsync(cancellationToken);
    }
}
