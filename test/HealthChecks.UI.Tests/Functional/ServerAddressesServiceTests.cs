using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

namespace HealthChecks.UI.Tests;

public class server_addresses_service_should
{
    // Based in AspNetCore TestServer test - https://github.com/aspnet/Hosting/pull/969
    [Fact]
    public void parse_correctly_relative_endpoint_uris()
    {
        var serverAddress = "http://localhost:5000";

        var host = new WebHostBuilder()
            .UseUrls(serverAddress)
            .ConfigureServices(services => services.AddSingleton<ServerAddressesService>())
            .Configure(app =>
            {
                app.ServerFeatures.Get<IServerAddressesFeature>().ShouldNotBeNull();

                var serverAddressService = app.ApplicationServices.GetRequiredService<ServerAddressesService>();

                serverAddressService.AbsoluteUriFromRelative("/health2")
                    .ShouldBe($"{serverAddress}/health2");

                serverAddressService.AbsoluteUriFromRelative("healthz")
                    .ShouldBe($"{serverAddress}/healthz");

                serverAddressService.AbsoluteUriFromRelative("/my/relative/url")
                   .ShouldBe($"{serverAddress}/my/relative/url");

                serverAddressService.AbsoluteUriFromRelative("segment1/segment2/segment3")
                 .ShouldBe($"{serverAddress}/segment1/segment2/segment3");
            });

        var featureCollection = new FeatureCollection();
        featureCollection.Set<IServerAddressesFeature>(new ServerAddressesFeature());

        var testServer = new TestServer(host, featureCollection);
    }
}
