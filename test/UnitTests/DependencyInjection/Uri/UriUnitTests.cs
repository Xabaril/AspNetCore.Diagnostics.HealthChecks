using HealthChecks.Uris;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.UriGroup
{
    public class uri_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("uri-group", typeof(UriHealthCheck), builder => builder.AddUrlGroup(
                new Uri("http://httpbin.org/status/200")));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-uri-group", typeof(UriHealthCheck), builder => builder.AddUrlGroup(
                new Uri("http://httpbin.org/status/200"), name: "my-uri-group"));
        }
    }
}
