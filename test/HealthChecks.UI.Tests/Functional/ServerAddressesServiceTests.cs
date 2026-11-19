using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace HealthChecks.UI.Tests;

public class server_addresses_service_should
{
    // Based in AspNetCore TestServer test - https://github.com/aspnet/Hosting/pull/969
    [Fact]
    public async Task parse_correctly_relative_endpoint_uris()
    {
        var serverAddress = "http://localhost:5000";

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(serverAddress);
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<ServerAddressesService>();
        await using var app = builder.Build();

        // Eski testteki gibi TestServer'a adres özelliği verilir; host başlarken "urls" ayarındaki adresi buraya yazar.
        var serverFeatures = app.GetTestServer().Features;
        serverFeatures.Set<IServerAddressesFeature>(new ServerAddressesFeature());

        await app.StartAsync();

        serverFeatures.Get<IServerAddressesFeature>().ShouldNotBeNull();

        var serverAddressService = app.Services.GetRequiredService<ServerAddressesService>();

        serverAddressService.AbsoluteUriFromRelative("/health2")
            .ShouldBe($"{serverAddress}/health2");

        serverAddressService.AbsoluteUriFromRelative("healthz")
            .ShouldBe($"{serverAddress}/healthz");

        serverAddressService.AbsoluteUriFromRelative("/my/relative/url")
           .ShouldBe($"{serverAddress}/my/relative/url");

        serverAddressService.AbsoluteUriFromRelative("segment1/segment2/segment3")
         .ShouldBe($"{serverAddress}/segment1/segment2/segment3");
    }
}
