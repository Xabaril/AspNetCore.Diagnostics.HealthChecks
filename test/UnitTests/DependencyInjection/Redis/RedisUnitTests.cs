using HealthChecks.Redis;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Redis
{
    public class redis_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("redis", typeof(RedisHealthCheck), build => build.AddRedis("connectionstring"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-redis", typeof(RedisHealthCheck), build => build.AddRedis("connectionstring", name: "my-redis"));
        }
    }
}
