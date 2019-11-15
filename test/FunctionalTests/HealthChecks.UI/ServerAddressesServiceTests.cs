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

namespace FunctionalTests.HealthChecks.UI
{
    public class server_addresses_service_should
    {
        [Fact]
        public void parse_correctly_relative_endpoint_uris()
        {
            var serverAddress = "https://localhost:9845";

            var host = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .UseKestrel()
                .ConfigureServices(services => services.AddHealthChecksUI(setupSettings: setup => setup.SetHealthCheckDatabaseConnectionString("Data Source=hcdb")));

            var featureCollection = new FeatureCollection();
            featureCollection.Set<IServerAddressesFeature>(new CustomServerAddressFeature(serverAddress));

            var testServer = new TestServer(host, featureCollection);
            var serverAddressService = testServer.Services.GetRequiredService<ServerAddressesService>();

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

    internal class CustomServerAddressFeature : IServerAddressesFeature
    {
        private List<string> _addresses = new List<string>();
        public CustomServerAddressFeature(params string[] addresses)
        {

            foreach (var address in addresses)
            {
                _addresses.Add(address);
            }
        }

        public ICollection<string> Addresses => _addresses;

        public bool PreferHostingUrls { get => true; set => throw new NotImplementedException(); }
    }
}