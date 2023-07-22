using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Data.Tests;

public class HealthChecksDbTests
{
    [Fact]
    public async Task HealthChecksDb_Primitive_Test()
    {
        var services = new ServiceCollection();
        services.AddDbContext<HealthChecksDb>(b => b.UseInMemoryDatabase(""));
        using var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<HealthChecksDb>();
        (await db.Configurations.ToListAsync().ConfigureAwait(false)).Count.ShouldBe(0);
        (await db.Executions.ToListAsync().ConfigureAwait(false)).Count.ShouldBe(0);
        (await db.Failures.ToListAsync().ConfigureAwait(false)).Count.ShouldBe(0);
        (await db.HealthCheckExecutionEntries.ToListAsync().ConfigureAwait(false)).Count.ShouldBe(0);
        (await db.HealthCheckExecutionHistories.ToListAsync().ConfigureAwait(false)).Count.ShouldBe(0);
    }
}
