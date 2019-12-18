using System.IO;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.UI;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace FunctionalTests.UI.Configuration
{
    public class UI_Configuration_should
    {
        [Fact]
        public void initialize_configuration_using_AddHealthChecksUI_setup_fluent_api()
        {
            var healthCheckName = "api1";
            var healthCheckUri = "http://api1/health";
            var webhookName = "webhook1";
            var webhookUri = "http://webhook1/sample";
            var webhookPayload = "payload1";
            var webhookRestorePayload = "restoredpayload1";
            var databaseConnection = "Data Source=healthchecksdb";
            var evaluationTimeInSeconds = 180;
            var minimumSeconds = 30;

            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI(setupSettings: settings =>
                    {
                        settings
                            .AddHealthCheckEndpoint(name: healthCheckName, uri: healthCheckUri)
                            .AddWebhookNotification(name: webhookName, uri: webhookUri, payload: webhookPayload,
                                restorePayload: webhookRestorePayload)
                            .SetEvaluationTimeInSeconds(evaluationTimeInSeconds)
                            .SetMinimumSecondsBetweenFailureNotifications(minimumSeconds)
                            .SetHealthCheckDatabaseConnectionString(databaseConnection);
                    });
                });

            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetService<IOptions<Settings>>().Value;

            UISettings.EvaluationTimeInSeconds.Should().Be(evaluationTimeInSeconds);
            UISettings.HealthCheckDatabaseConnectionString.Should().Be(databaseConnection);
            UISettings.MinimumSecondsBetweenFailureNotifications.Should().Be(minimumSeconds);

            UISettings.Webhooks.Count.Should().Be(1);
            UISettings.HealthChecks.Count.Should().Be(1);

            var healthcheck = UISettings.HealthChecks[0];
            healthcheck.Name.Should().Be(healthCheckName);
            healthcheck.Uri.Should().Be(healthCheckUri);

            var webhook = UISettings.Webhooks[0];
            webhook.Name.Should().Be(webhookName);
            webhook.Uri.Should().Be(webhookUri);
            webhook.Payload.Should().Be(webhookPayload);
            webhook.RestoredPayload.Should().Be(webhookRestorePayload);
        }

        [Fact]
        public void load_ui_settings_from_configuration_key()
        {
            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureAppConfiguration(conf =>
                {
                    conf.Sources.Clear();
                    conf.AddJsonFile("HealthChecks.UI/Configuration/appsettings.json", false);
                }).ConfigureServices(services => { services.AddHealthChecksUI(); });


            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetService<IOptions<Settings>>().Value;


            UISettings.EvaluationTimeInSeconds.Should().Be(20);
            UISettings.MinimumSecondsBetweenFailureNotifications.Should().Be(120);
            UISettings.HealthCheckDatabaseConnectionString.Should().Be("Data Source=healthchecksdb");

            UISettings.HealthChecks.Count.Should().Be(1);
            UISettings.Webhooks.Count.Should().Be(1);

            var healthcheck = UISettings.HealthChecks[0];
            healthcheck.Name.Should().Be("api1");
            healthcheck.Uri.Should().Be("http://api1/healthz");


            var webhook = UISettings.Webhooks[0];
            webhook.Name.Should().Be("webhook1");
            webhook.Uri.Should().Be("http://webhook1");
            webhook.Payload.Should().Be("payload");
            webhook.RestoredPayload.Should().Be("restoredpayload");
        }

        [Fact]
        public void support_combined_configuration_from_fluent_api_and_settings_key()
        {
            var healthCheckName = "api2";
            var healthCheckUri = "http://api2/healthz";
            var webhookName = "webhook2";
            var webhookUri = "http://webhook2";
            var webhookPayload = "payload1";

            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureAppConfiguration(conf =>
                {
                    conf.Sources.Clear();
                    conf.AddJsonFile("HealthChecks.UI/Configuration/appsettings.json", false);
                }).ConfigureServices(services =>
                {
                    services.AddHealthChecksUI(setupSettings: setup =>
                    {
                        setup
                            .AddHealthCheckEndpoint(name: healthCheckName, uri: healthCheckUri)
                            .AddWebhookNotification(name: webhookName, uri: webhookUri, payload: webhookPayload)
                            .SetMinimumSecondsBetweenFailureNotifications(200);
                    });
                });

            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetService<IOptions<Settings>>().Value;

            UISettings.MinimumSecondsBetweenFailureNotifications.Should().Be(200);
            UISettings.EvaluationTimeInSeconds.Should().Be(20);
            UISettings.HealthCheckDatabaseConnectionString.Should().NotBeEmpty();
            UISettings.Webhooks.Count.Should().Be(2);
            UISettings.HealthChecks.Count.Should().Be((2));

            var healthCheck1 = UISettings.HealthChecks[0];
            var healthCheck2 = UISettings.HealthChecks[1];
            var webHook1 = UISettings.Webhooks[0];
            var webHook2 = UISettings.Webhooks[1];

            healthCheck1.Name.Should().Be("api1");
            healthCheck1.Uri.Should().Be("http://api1/healthz");
            healthCheck2.Name.Should().Be("api2");
            healthCheck2.Uri.Should().Be("http://api2/healthz");

            webHook1.Name.Should().Be("webhook1");
            webHook1.Uri.Should().Be("http://webhook1");
            webHook2.Name.Should().Be(webhookName);
            webHook2.Uri.Should().Be(webhookUri);
        }

        [Fact]
        public void should_register_configured_http_client_and_http_message_handler_into_client()
        {
            var apiHandlerConfigured = false;
            var apiClientConfigured = false;
            var webhookHandlerConfigured = false;
            var webhookClientConfigured = false;

            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureAppConfiguration(conf =>
                {
                    conf.Sources.Clear();
                    conf.AddJsonFile("HealthChecks.UI/Configuration/appsettings.json", false);
                })
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI(setupSettings: setup =>
                    {
                        setup.ConfigureApiEndpointHttpclient((sp, client) =>
                        {
                            apiClientConfigured = true;
                        })
                        .UseApiEndpointHttpMessageHandler(sp =>
                            {
                                apiHandlerConfigured = true;
                                return new HttpClientHandler
                                {
                                    Proxy = new WebProxy("http://proxy:8080")
                                };
                            })
                        .ConfigureWebhooksEndpointHttpclient((sp, client) =>
                        {
                            webhookClientConfigured = true;
                        })
                        .UseWebhookEndpointHttpMessageHandler(sp =>
                        {
                            webhookHandlerConfigured = true;
                            return new HttpClientHandler()
                            {
                                Properties =
                                {
                                    ["prop"] = "value"
                                }
                            };
                        });
                    });
                }).Build();

            var clientFactory = webhost.Services.GetService<IHttpClientFactory>();
            var apiClient = clientFactory.CreateClient(Keys.HEALTH_CHECK_HTTP_CLIENT_NAME);
            var webhookClient = clientFactory.CreateClient(Keys.HEALTH_CHECK_WEBHOOK_HTTP_CLIENT_NAME);

            apiHandlerConfigured.Should().BeTrue();
            apiClientConfigured.Should().BeTrue();
            webhookHandlerConfigured.Should().BeTrue();
            webhookClientConfigured.Should().BeTrue();
        }

        [Fact]
        public void register_server_addresses_service_to_resolve_relative_uris_using_endpoints()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecksUI(setupSettings: setup => setup.SetHealthCheckDatabaseConnectionString("Data Source=hcdb"))
                    .AddRouting();

                }).Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(config =>
                    {
                        config.MapHealthChecksUI();
                    });

                });

            var serviceProvider = webHostBuilder.Build().Services;
            var serverAddressesService = serviceProvider.GetRequiredService<ServerAddressesService>();

            serverAddressesService.Should().NotBeNull();
            serverAddressesService.Addresses.Should().NotBeNull();
        }

        [Fact]
        public void register_server_addresses_service_to_resolve_relative_uris_using_application_builder()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.
                    AddHealthChecksUI(setupSettings: setup => setup.SetHealthCheckDatabaseConnectionString("Data Source=hcdb"));

                }).Configure(app => app.UseHealthChecksUI());

            var serviceProvider = webHostBuilder.Build().Services;
            var serverAddressesService = serviceProvider.GetRequiredService<ServerAddressesService>();

            serverAddressesService.Should().NotBeNull();
            serverAddressesService.Addresses.Should().NotBeNull();
        }
    }
}