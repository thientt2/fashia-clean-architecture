using Fashia.Application.Common.Options;
using NUnit.Framework;
using Shouldly;

namespace Fashia.Application.UnitTests.Common.Options;

public class IdempotencyOptionsTests
{
    [Test]
    public void ShouldDefaultRetentionToTwentyFourHours()
    {
        var options = new IdempotencyOptions();

        options.EffectiveRetentionPeriod.ShouldBe(TimeSpan.FromHours(24));
        options.EffectiveCleanupInterval.ShouldBe(TimeSpan.FromHours(1));
        options.CleanupEnabled.ShouldBeTrue();
    }

    [Test]
    public void ShouldUseSafeDefaultsWhenConfiguredDurationsAreNotPositive()
    {
        var options = new IdempotencyOptions
        {
            RetentionPeriod = TimeSpan.Zero,
            CleanupInterval = TimeSpan.FromMinutes(-1),
        };

        options.EffectiveRetentionPeriod.ShouldBe(TimeSpan.FromHours(24));
        options.EffectiveCleanupInterval.ShouldBe(TimeSpan.FromHours(1));
    }
}
