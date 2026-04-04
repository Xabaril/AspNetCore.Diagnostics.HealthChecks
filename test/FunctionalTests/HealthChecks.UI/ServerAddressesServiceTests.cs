using Microsoft.AspNetCore.Hosting.Server.Features;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using HealthChecks.UI.Core;
using FluentAssertions;

namespace FunctionalTests.HealthChecks.UI
{
    public class server_addresses_service_should
    {
        // Based in AspNetCore TestServer test - https://github.com/aspnet/Hosting/pull/969
        [Fact]
        public async Task parse_correctly_relative_endpoint_uris()
        {
            var serverAddress = "http://localhost:5000";

            var host = WebApplication.CreateBuilder();
            host.WebHost.UseUrls(serverAddress);
            host.Services.AddSingleton<ServerAddressesService>();
            host.WebHost.UseTestServer();
            await using var app = host.Build();
            await app.StartAsync();

            app.ServerFeatures.Get<IServerAddressesFeature>().Should().NotBeNull();

            var serverAddressService = app.Services.GetRequiredService<ServerAddressesService>();

            serverAddressService.AbsoluteUriFromRelative("/health2")
                .Should().Be($"{serverAddress}/health2");

            serverAddressService.AbsoluteUriFromRelative("healthz")
                .Should().Be($"{serverAddress}/healthz");

            serverAddressService.AbsoluteUriFromRelative("/my/relative/url")
               .Should().Be($"{serverAddress}/my/relative/url");

            serverAddressService.AbsoluteUriFromRelative("segment1/segment2/segment3")
             .Should().Be($"{serverAddress}/segment1/segment2/segment3");
        }
    }
}