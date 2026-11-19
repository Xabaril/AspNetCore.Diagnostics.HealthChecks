using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Data.Tests;

public class HealthChecksDbTests
{
    [Fact]
    public async Task HealthChecksDb_Primitive_Test()
    {
        var services = new ServiceCollection();
        // Güncel EF Core boş veritabanı adını reddediyor; test başına benzersiz bir ad verilir.
        var databaseName = $"{nameof(HealthChecksDb_Primitive_Test)}-{Guid.NewGuid():N}";
        services.AddDbContext<HealthChecksDb>(b => b.UseInMemoryDatabase(databaseName));
        using var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<HealthChecksDb>();
        (await db.Configurations.ToListAsync()).Count.ShouldBe(0);
        (await db.Executions.ToListAsync()).Count.ShouldBe(0);
        (await db.Failures.ToListAsync()).Count.ShouldBe(0);
        (await db.HealthCheckExecutionEntries.ToListAsync()).Count.ShouldBe(0);
        (await db.HealthCheckExecutionHistories.ToListAsync()).Count.ShouldBe(0);
    }
}
