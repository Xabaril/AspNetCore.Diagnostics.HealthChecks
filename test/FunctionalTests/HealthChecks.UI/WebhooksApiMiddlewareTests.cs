using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.UI
{
    public class webhooks_api_middleware_should
    {
        [Fact]
        public async Task not_display_the_whole_host_uri_to_prevent_secrets_leaks()
        {
            var webhook1 = "https://webhookserver.com/segment?code=23325";
            var webhook2 = "http://sampleserver.com/segment?param1=bar&param2=foo";
            var webhooksPath = "/wh";

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddHealthChecksUI(setupSettings: setup =>
                     {
                         setup.SetHealthCheckDatabaseConnectionString("Data Source=hcdb");
                         setup.AddWebhookNotification("webhook1", webhook1, "{}");
                         setup.AddWebhookNotification("webhook1", webhook2, "{}");
                     });

                }).Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(setup =>
                    {
                        setup.MapHealthChecksUI(setup => setup.WebhookPath = webhooksPath);
                    });
                });

            var server = new TestServer(webHostBuilder);
            var result = await server.CreateClient().GetStringAsync(webhooksPath);

            var webhooks = JArray.Parse(result);

            Uri.TryCreate(webhook1, UriKind.Absolute, out var webhook1Uri);
            Uri.TryCreate(webhook2, UriKind.Absolute, out var webhook2Uri);

            webhooks[0].Value<string>("host").Should().Be(webhook1Uri.Host);
            webhooks[1].Value<string>("host").Should().Be(webhook2Uri.Host);

        }
    }
}
