using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using HealthChecks.UI.Core;
using FluentAssertions;
using FunctionalTests.Base;
using System.Threading.Tasks;

namespace FunctionalTests.HealthChecks.UI
{
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
                    app.ServerFeatures.Get<IServerAddressesFeature>().Should().NotBeNull();

                    var serverAddressService = app.ApplicationServices.GetRequiredService<ServerAddressesService>();

                    serverAddressService.AbsoluteUriFromRelative("/health2")
                        .Should().Be($"{serverAddress}/health2");

                    serverAddressService.AbsoluteUriFromRelative("healthz")
                        .Should().Be($"{serverAddress}/healthz");

                    serverAddressService.AbsoluteUriFromRelative("/my/relative/url")
                       .Should().Be($"{serverAddress}/my/relative/url");

                    serverAddressService.AbsoluteUriFromRelative("segment1/segment2/segment3")
                     .Should().Be($"{serverAddress}/segment1/segment2/segment3");

                });

            var featureCollection = new FeatureCollection();
            featureCollection.Set<IServerAddressesFeature>(new ServerAddressesFeature());

            var testServer = new TestServer(host, featureCollection);
        }
        
    }

}