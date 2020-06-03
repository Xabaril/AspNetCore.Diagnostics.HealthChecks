using FunctionalTests.Base;
using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using YamlDotNet.Core.Tokens;

namespace FunctionalTests.HealthChecks.UI.Configuration
{
    [Collection("execution")]
    public class UI_configuration_should
    {
        [Fact]
        public Task configure_custom_http_client_handler()
        {
            var keyName = "prop1";
            var valueName = "prop1value";

            var tracer = Substitute.For<MessageHandlerTracer>();
            var handler = new ClientHandler(tracer, new Dictionary<string, string> { [keyName] = valueName });
            var hostReset = new ManualResetEventSlim(false);

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                    .AddRouting()
                    .AddSingleton<IHealthCheckCollectorInterceptor>(sp => new TestCollectorInterceptor(hostReset))
                    .AddHealthChecksUI(setup =>
                    {
                        setup.AddHealthCheckEndpoint("endpoint1", "https://httpstat.us/200");
                        setup.UseApiEndpointHttpMessageHandler(sp => handler);
                    }).AddInMemoryStorage();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(setup =>
                    {
                        setup.MapHealthChecksUI();
                    });
                });

            var server = new TestServer(builder);
            hostReset.Wait(3000);

            tracer.Received().Log(keyName, valueName);

            return Task.CompletedTask;
        }

        [Fact]
        public Task configure_custom_delegating_handlers()
        {
            var hostReset = new ManualResetEventSlim(false);
            var tracer = Substitute.For<MessageHandlerTracer>();

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                    .AddRouting()
                    .AddSingleton<IHealthCheckCollectorInterceptor>(sp => new TestCollectorInterceptor(hostReset))
                    .AddTransient(sp => new CustomDelegatingHandler(tracer))
                    .AddTransient(sp => new CustomDelegatingHandler2(tracer))
                    .AddHealthChecksUI(setup =>
                    {
                        setup.AddHealthCheckEndpoint("endpoint1", "https://httpstat.us/200");
                        setup.UseApiEndpointDelegatingHandler<CustomDelegatingHandler>();
                        setup.UseApiEndpointDelegatingHandler<CustomDelegatingHandler2>();

                    }).AddInMemoryStorage();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(setup =>
                    {
                        setup.MapHealthChecksUI();
                    });
                });

            var server = new TestServer(builder);

            hostReset.Wait(3000);

            tracer.Received().Log(nameof(CustomDelegatingHandler), "SendAsync");
            tracer.Received().Log(nameof(CustomDelegatingHandler2), "SendAsync");

            return Task.CompletedTask;
        }

        public class ClientHandler : HttpClientHandler
        {
            private readonly MessageHandlerTracer _tracer;

            public ClientHandler(MessageHandlerTracer tracer, IDictionary<string, string> properties)
            {
                _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
                _ = properties ?? throw new ArgumentNullException(nameof(properties));

                foreach (var kv in properties)
                {
                    Properties[kv.Key] = kv.Value;
                }
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                foreach (var prop in Properties)
                {
                    _tracer.Log(prop.Key, prop.Value.ToString());
                }

                return base.SendAsync(request, cancellationToken);
            }
        }

        public class CustomDelegatingHandler : DelegatingHandler
        {
            private readonly MessageHandlerTracer _tracer;

            public CustomDelegatingHandler(MessageHandlerTracer tracer)
            {
                _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _tracer.Log(nameof(CustomDelegatingHandler), nameof(SendAsync));
                return base.SendAsync(request, cancellationToken);
            }
        }

        public class CustomDelegatingHandler2 : DelegatingHandler
        {
            private readonly MessageHandlerTracer _tracer;

            public CustomDelegatingHandler2(MessageHandlerTracer tracer)
            {
                _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _tracer.Log(nameof(CustomDelegatingHandler2), nameof(SendAsync));
                return base.SendAsync(request, cancellationToken);
            }
        }

        public abstract class MessageHandlerTracer
        {
            public abstract void Log(string key, string value);
        }
    }
}
