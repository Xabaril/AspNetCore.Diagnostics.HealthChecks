using HealthChecks.IdSvr;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.IdSvr
{
    public class idsrv_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("idsvr", typeof(IdSvrHealthCheck), builder => builder.AddIdentityServer(
                new Uri("http://myidsvr")));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-idsvr-group", typeof(IdSvrHealthCheck), builder => builder.AddIdentityServer(
                new Uri("http://myidsvr"), name: "my-idsvr-group"));
        }
    }
}
