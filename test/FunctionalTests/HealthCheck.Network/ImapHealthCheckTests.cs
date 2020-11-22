using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.Network;
using HealthChecks.Network.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.Network
{
    [Collection("execution")]
    public class imap_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        //Host and login account to fast switch tests against different server
        private const string _host = "localhost";
        private const string _validAccount = "admin@healthchecks.com";
        private const string _validPassword = "beatpulse";

        public imap_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_connecting_to_imap_ssl_port_without_login()
        {
            var webHostBuilder = new WebHostBuilder()
            .UseStartup<DefaultStartup>()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                 .AddImapHealthCheck(setup =>
                 {
                     setup.Host = _host;
                     setup.Port = 993;
                     setup.AllowInvalidRemoteCertificates = true;
                 }, tags: new string[] { "imap" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("imap")
                });
            });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_connecting_to_imap_ssl_and_login_with_correct_account()
        {
            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddImapHealthCheck(setup =>
                    {
                        setup.Host = _host;
                        setup.Port = 993;
                        setup.AllowInvalidRemoteCertificates = true;
                        setup.LoginWith(_validAccount, _validPassword);
                    }, tags: new string[] { "imap" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("imap")
                   });
               });


            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_when_connecting_to_imap_ssl_and_login_with_an_incorrect_account()
        {
            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddImapHealthCheck(setup =>
                    {
                        setup.Host = _host;
                        setup.Port = 993;
                        setup.ConnectionType = ImapConnectionType.SSL_TLS;
                        setup.AllowInvalidRemoteCertificates = true;
                        setup.LoginWith("invalid@healthchecks.com", "invalidpassword");
                    }, tags: new string[] { "imap" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("imap")
                   });
               });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_connecting_to_imap_ssl_with_a_correct_account_checking_an_existing_folder()
        {
            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddImapHealthCheck(setup =>
                    {
                        setup.Host = _host;
                        setup.Port = 143;
                        setup.AllowInvalidRemoteCertificates = true;
                        setup.LoginWith(_validAccount, _validPassword);
                        setup.CheckFolderExists("INBOX");
                    }, tags: new string[] { "imap" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("imap")
                   });
               });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }


        [SkipOnAppVeyor]
        public async Task be_unhealthy_when_connecting_to_imap_ssl_with_a_correct_account_checking_an_non_existing_folder()
        {
            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddImapHealthCheck(setup =>
                    {
                        setup.Host = _host;
                        setup.Port = 993;
                        setup.AllowInvalidRemoteCertificates = true;
                        setup.LoginWith(_validAccount, _validPassword);
                        setup.CheckFolderExists("INVALIDFOLDER");
                    }, tags: new string[] { "imap" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("imap")
                   });
               });


            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_imap_connects_to_starttls_port()
        {
            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddImapHealthCheck(setup =>
                   {
                       setup.Host = _host;
                       setup.Port = 143;
                       setup.AllowInvalidRemoteCertificates = true;
                   }, tags: new string[] { "imap" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("imap")
                  });
              });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_imap_performs_login_using_starttls_handshake()
        {
            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddImapHealthCheck(setup =>
                   {
                       setup.Host = _host;
                       setup.Port = 143;
                       setup.AllowInvalidRemoteCertificates = true;
                       setup.LoginWith(_validAccount, _validPassword);
                   }, tags: new string[] { "imap" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("imap")
                  });
              });


            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_imap_performs_login_and_folder_check_using_starttls_handshake()
        {
            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddImapHealthCheck(setup =>
                   {
                       setup.Host = _host;
                       setup.Port = 143;
                       setup.ConnectionType = ImapConnectionType.STARTTLS;
                       setup.AllowInvalidRemoteCertificates = true;
                       setup.LoginWith(_validAccount, _validPassword);
                       setup.CheckFolderExists("INBOX");
                   }, tags: new string[] { "imap" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("imap")
                  });
              });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_when_using_configuration_auto_with_an_invalid_imap_port()
        {
            var webHostBuilder = new WebHostBuilder()
             .UseStartup<DefaultStartup>()
             .ConfigureServices(services =>
             {
                 services.AddHealthChecks()
                  .AddImapHealthCheck(setup =>
                  {
                      setup.Host = _host;
                      setup.Port = 135;
                  }, tags: new string[] { "imap" });
             })
             .Configure(app =>
             {
                 var options = new HealthCheckOptions()
                 {
                     Predicate = r => r.Tags.Contains("imap")
                 };
                 options.ResultStatusCodes[HealthStatus.Unhealthy] = (int)HttpStatusCode.ServiceUnavailable;

                 app.UseHealthChecks("/health", options);
             });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task respect_configured_timeout_and_throw_operation_cancelled_exception()
        {
            var options = new ImapHealthCheckOptions() { Host = "invalid", Port = 993 };
            options.LoginWith("user", "pass");

            var imapHealthCheck = new ImapHealthCheck(options);

            var result = await imapHealthCheck.CheckHealthAsync(new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("imap", instance: imapHealthCheck, failureStatus: HealthStatus.Degraded,
                    null, timeout: null)
            }, new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);

            result.Exception.Should().BeOfType<OperationCanceledException>();
        }
    }
}
