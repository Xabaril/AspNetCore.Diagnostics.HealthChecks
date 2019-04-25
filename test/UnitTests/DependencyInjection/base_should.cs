using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Xunit;

public abstract class base_should
{
    protected static void ShouldPass(Action<IHealthChecksBuilder> configure, Action<HealthCheckRegistration, IHealthCheck> assertion)
    {
        var services = new ServiceCollection();
        configure(services.AddHealthChecks());

        var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        assertion(registration, check);
    }

    protected static void ShouldPass(string expectedName, Type expectedType, Action<IHealthChecksBuilder> configure)
    {
        ShouldPass(configure, (registration, check) =>
        {
            registration.Name.Should().Be(expectedName);
            check.GetType().Should().Be(expectedType);
        });
    }

    protected static void ShouldThrow<TException>(Action<IHealthChecksBuilder> configure) where TException : Exception
    {
        var services = new ServiceCollection();
        configure(services.AddHealthChecks());

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        Assert.Throws<TException>(() => registration.Factory(serviceProvider));
    }
}
