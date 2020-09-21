using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Threading.Tasks;
using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace FunctionalTests.HealthChecks.UI
{
    public class ui_oidc_authentication
    {
        private static string authorityUrl = "https://demo.identityserver.io";
        private static string clientId = "m2m";
        private static string scope = "api";
        private static string secret = "secret";

        [Fact]
        public async Task should_return_ok_with_oidc_authentication_enabled_and_a_valid_bearer_token()
        {
            var builder = BuildUIWebhostBuilderWithSetup(enableAuthentication: true, setup =>
            {
                setup.AddOidcClientAuthentication(setup =>
                {
                    setup.Authority = authorityUrl;
                    setup.Scope = scope;
                    setup.ClientId = clientId;
                });
            });

            var server = new TestServer(builder);

            using var identityClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

            var document = await identityClient.GetDiscoveryDocumentAsync(authorityUrl);

            var credentials = await identityClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = document.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = secret,
                Scope = scope,
                GrantType = OidcConstants.GrantTypes.AuthorizationCode
            });

            using var client = server.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", credentials.AccessToken);
            var response = await client.GetAsync("/healthchecks-api");

            response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }


        [Fact]
        public async Task should_return_unauthorized_with_no_valid_bearer_token()
        {
            var builder = BuildUIWebhostBuilderWithSetup(enableAuthentication: true, setup =>
            {
                setup.AddOidcClientAuthentication(setup =>
                {
                    setup.Authority = authorityUrl;
                    setup.Scope = scope;
                    setup.ClientId = clientId;
                });
            });

            var server = new TestServer(builder);

            var response = await server.CreateRequest("/healthchecks-api").GetAsync();
            response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task should_return_ok_with_no_oidc_authentication_enabled()
        {
            var builder = BuildUIWebhostBuilderWithSetup(enableAuthentication: false, setup => { });
            var server = new TestServer(builder);

            var response = await server.CreateRequest("/healthchecks-api").GetAsync();
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }


        private static IWebHostBuilder BuildUIWebhostBuilderWithSetup(bool enableAuthentication,
            Action<Settings> configureSetings)
        {
            return new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services
                        .AddRouting();

                    if (enableAuthentication)
                    {
                        services
                            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer(config =>
                            {
                                config.Authority = authorityUrl;
                                config.Audience = "api";
                                config.SaveToken = true;
                            });
                    }

                    services.AddAuthorization(config =>
                        {
                            config.DefaultPolicy =
                                new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                        })
                        .AddHealthChecks()
                        .AddCheck("check1", () => HealthCheckResult.Healthy())
                        .Services
                        .AddHealthChecksUI(setup => { configureSetings(setup); })
                        .AddInMemoryStorage(databaseName: "OidcTests");
                }).Configure(app =>
                {
                    app.UseRouting();
                    if (enableAuthentication)
                    {
                        app.UseAuthentication();
                    }

                    app.UseAuthorization()
                        .UseEndpoints(config =>
                        {
                            config.MapHealthChecks("/health", new HealthCheckOptions
                            {
                                Predicate = r => true,
                                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                            });

                            config.MapHealthChecksUI();
                        });
                });
        }
    }
}