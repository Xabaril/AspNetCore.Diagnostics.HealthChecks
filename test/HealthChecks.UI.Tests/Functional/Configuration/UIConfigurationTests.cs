using System.Net;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.UI.Tests
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
            var evaluationTimeInSeconds = 180;
            var minimumSeconds = 30;

            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI(setupSettings: settings =>
                    {
                        settings
                            .DisableDatabaseMigrations()
                            .AddHealthCheckEndpoint(name: healthCheckName, uri: healthCheckUri)
                            .AddWebhookNotification(name: webhookName, uri: webhookUri, payload: webhookPayload,
                                restorePayload: webhookRestorePayload)
                            .SetEvaluationTimeInSeconds(evaluationTimeInSeconds)
                            .SetMinimumSecondsBetweenFailureNotifications(minimumSeconds);
                    }).AddInMemoryStorage();
                });

            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;

            UISettings.EvaluationTimeInSeconds.ShouldBe(evaluationTimeInSeconds);
            UISettings.MinimumSecondsBetweenFailureNotifications.ShouldBe(minimumSeconds);

            UISettings.Webhooks.Count.ShouldBe(1);
            UISettings.HealthChecks.Count.ShouldBe(1);

            var healthcheck = UISettings.HealthChecks[0];
            healthcheck.Name.ShouldBe(healthCheckName);
            healthcheck.Uri.ShouldBe(healthCheckUri);

            var webhook = UISettings.Webhooks[0];
            webhook.Name.ShouldBe(webhookName);
            webhook.Uri.ShouldBe(webhookUri);
            webhook.Payload.ShouldBe(webhookPayload);
            webhook.RestoredPayload.ShouldBe(webhookRestorePayload);
        }

        [Fact]
        public void load_ui_settings_from_configuration_key()
        {
            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureAppConfiguration(conf =>
                {
                    conf.Sources.Clear();
                    var path = Path.Combine("Functional", "Configuration", "appsettings.json");
                    conf.AddJsonFile(path, false);
                }).ConfigureServices(services => services.AddHealthChecksUI());

            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;

            UISettings.EvaluationTimeInSeconds.ShouldBe(20);
            UISettings.MinimumSecondsBetweenFailureNotifications.ShouldBe(120);
            UISettings.HealthChecks.Count.ShouldBe(1);
            UISettings.Webhooks.Count.ShouldBe(1);

            var healthcheck = UISettings.HealthChecks[0];
            healthcheck.Name.ShouldBe("api1");
            healthcheck.Uri.ShouldBe("http://api1/healthz");

            var webhook = UISettings.Webhooks[0];
            webhook.Name.ShouldBe("webhook1");
            webhook.Uri.ShouldBe("http://webhook1");
            webhook.Payload.ShouldBe("payload");
            webhook.RestoredPayload.ShouldBe("restoredpayload");
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
                    var path = Path.Combine("Functional", "Configuration", "appsettings.json");
                    conf.AddJsonFile(path, false);
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
            var UISettings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;

            UISettings.MinimumSecondsBetweenFailureNotifications.ShouldBe(200);
            UISettings.EvaluationTimeInSeconds.ShouldBe(20);
            UISettings.Webhooks.Count.ShouldBe(2);
            UISettings.HealthChecks.Count.ShouldBe((2));

            var healthCheck1 = UISettings.HealthChecks[0];
            var healthCheck2 = UISettings.HealthChecks[1];
            var webHook1 = UISettings.Webhooks[0];
            var webHook2 = UISettings.Webhooks[1];

            healthCheck1.Name.ShouldBe("api1");
            healthCheck1.Uri.ShouldBe("http://api1/healthz");
            healthCheck2.Name.ShouldBe("api2");
            healthCheck2.Uri.ShouldBe("http://api2/healthz");

            webHook1.Name.ShouldBe("webhook1");
            webHook1.Uri.ShouldBe("http://webhook1");
            webHook2.Name.ShouldBe(webhookName);
            webHook2.Uri.ShouldBe(webhookUri);
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
                    var path = Path.Combine("Functional", "Configuration", "appsettings.json");
                    conf.AddJsonFile(path, false);
                })
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI(setupSettings: setup =>
                    {
                        setup.ConfigureApiEndpointHttpclient((sp, client) => apiClientConfigured = true)
                        .UseApiEndpointHttpMessageHandler(sp =>
                            {
                                apiHandlerConfigured = true;
                                return new HttpClientHandler
                                {
                                    Proxy = new WebProxy("http://proxy:8080")
                                };
                            })
                        .ConfigureWebhooksEndpointHttpclient((sp, client) => webhookClientConfigured = true)
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

            var clientFactory = webhost.Services.GetRequiredService<IHttpClientFactory>();
            var apiClient = clientFactory.CreateClient(Keys.HEALTH_CHECK_HTTP_CLIENT_NAME);
            var webhookClient = clientFactory.CreateClient(Keys.HEALTH_CHECK_WEBHOOK_HTTP_CLIENT_NAME);

            apiHandlerConfigured.ShouldBeTrue();
            apiClientConfigured.ShouldBeTrue();
            webhookHandlerConfigured.ShouldBeTrue();
            webhookClientConfigured.ShouldBeTrue();
        }

        [Fact]
        public void register_server_addresses_service_to_resolve_relative_uris_using_endpoints()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .UseKestrel()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecksUI()
                    .Services
                    .AddRouting();

                }).Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(config => config.MapHealthChecksUI());

                });

            var serviceProvider = webHostBuilder.Build().Services;
            var serverAddressesService = serviceProvider.GetRequiredService<ServerAddressesService>();

            serverAddressesService.ShouldNotBeNull();
            serverAddressesService.Addresses.ShouldNotBeNull();
        }

        [Fact]
        public void register_server_addresses_service_to_resolve_relative_uris_using_application_builder()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .UseKestrel()
                .ConfigureServices(services =>
                {
                    services.
                    AddHealthChecksUI();

                }).Configure(app => app.UseHealthChecksUI());

            var serviceProvider = webHostBuilder.Build().Services;
            var serverAddressesService = serviceProvider.GetRequiredService<ServerAddressesService>();

            serverAddressesService.ShouldNotBeNull();
            serverAddressesService.Addresses.ShouldNotBeNull();
        }

        [Fact]
        public void have_enabled_database_migrations_by_default()
        {
            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI()
                    .AddInMemoryStorage();
                });

            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;

            UISettings.DisableMigrations.ShouldBe(false);
        }

        [Fact]
        public void allow_disable_running_database_migrations_in_ui_setup()
        {
            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecksUI(setup => setup.DisableDatabaseMigrations())
                    .AddInMemoryStorage();
                });

            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;

            UISettings.DisableMigrations.ShouldBe(true);
        }

        [Fact]
        public void allow_disable_running_database_migrations_using_configuration_providers()
        {
            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                    {
                        new KeyValuePair<string, string?>("HealthChecksUI:DisableMigrations", "true")
                    });
                })
                .ConfigureServices(services =>
                {
                    services
                    .AddHealthChecksUI()
                    .AddInMemoryStorage();
                });

            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;

            UISettings.DisableMigrations.ShouldBe(true);
        }

        [Fact]
        public async Task support_configuring_page_title()
        {
            const string pageTitle = "My Health Checks UI";

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddRouting()
                        .AddHealthChecksUI();
                })
                .Configure(app =>
                {
                    app
                        .UseRouting()
                        .UseEndpoints(setup =>
                        {
                            setup.MapHealthChecksUI(options =>
                            {
                                options.PageTitle = pageTitle;
                            });
                        });
                });

            var server = new TestServer(builder);
            var options = server.Services.GetRequiredService<IOptions<Configuration.Options>>().Value;
            var response = await server.CreateRequest(options.UIPath).GetAsync().ConfigureAwait(false);
            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            html.ShouldContain($"<title>{pageTitle}</title>");
        }
    }
}
