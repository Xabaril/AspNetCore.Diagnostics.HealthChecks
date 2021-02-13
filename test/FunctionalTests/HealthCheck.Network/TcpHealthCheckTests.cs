using FluentAssertions;
using HealthChecks.Network;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.Network
{
    [Collection("execution")]
    public class tcp_healthcheck_should
    {
        [Fact]
        public async Task respect_configured_timeout_and_throw_operation_cancelled_exception()
        {
            var options = new TcpHealthCheckOptions();
            options.AddHost("invalid", 5555);

            var tcpHealthCheck = new TcpHealthCheck(options);

            var result = await tcpHealthCheck.CheckHealthAsync(new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("tcp", instance: tcpHealthCheck, failureStatus: HealthStatus.Degraded,
                    null, timeout: null)
            }, new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);

            result.Exception.Should().BeOfType<OperationCanceledException>();
        }
    }
}
