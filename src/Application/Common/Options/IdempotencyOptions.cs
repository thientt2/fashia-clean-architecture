namespace Fashia.Application.Common.Options;

public sealed class IdempotencyOptions
{
    private static readonly TimeSpan DefaultRetentionPeriod = TimeSpan.FromHours(24);
    private static readonly TimeSpan DefaultCleanupInterval = TimeSpan.FromHours(1);

    public const string SectionName = "Idempotency";

    public TimeSpan RetentionPeriod { get; init; } = DefaultRetentionPeriod;

    public TimeSpan CleanupInterval { get; init; } = DefaultCleanupInterval;

    public bool CleanupEnabled { get; init; } = true;

    public TimeSpan EffectiveRetentionPeriod =>
        RetentionPeriod > TimeSpan.Zero ? RetentionPeriod : DefaultRetentionPeriod;

    public TimeSpan EffectiveCleanupInterval =>
        CleanupInterval > TimeSpan.Zero ? CleanupInterval : DefaultCleanupInterval;
}
