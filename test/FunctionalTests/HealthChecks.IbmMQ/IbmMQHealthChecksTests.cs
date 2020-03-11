using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Collections;
using IBM.WMQ;

namespace FunctionalTests.HealthChecks.IbmMQ
{

  [Collection("execution")]
  public class ibmmq_healthcheck_should
  {
    private readonly ExecutionFixture _fixture;

    // Define the name of the queue manager to use (applies to all connections)
    const string qManager = "QM.TEST.01";

    // Define the name of your host connection (applies to client connections only)
    const string hostName = "localhost(1414)";

    const string wrongHostName = "localhost(1417)";

    // Define the name of the channel to use (applies to client connections only)
    const string channel = "DEV.APP.SVRCONN";

    // Define the user name.
    const string user = "app";

    // Define the password.
    const string password = "12345678";

    public ibmmq_healthcheck_should(ExecutionFixture fixture)
    {
      _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }


    [SkipOnAppVeyor]
    public async Task be_healthy_if_ibmmq_is_available()
    {
      Hashtable properties = new Hashtable();
      properties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
      properties.Add(MQC.CHANNEL_PROPERTY, channel);
      properties.Add(MQC.CONNECTION_NAME_PROPERTY, hostName);
      properties.Add(MQC.USER_ID_PROPERTY, user);
      properties.Add(MQC.PASSWORD_PROPERTY, password);

      var webHostBuilder = new WebHostBuilder()
           .UseStartup<DefaultStartup>()
           .ConfigureServices(services =>
           {
             services.AddHealthChecks().
                AddIbmMQ(qManager, properties , tags: new string[] { "ibmmq" });
           })
           .Configure(app =>
           {
             app.UseHealthChecks("/health", new HealthCheckOptions()
             {
               Predicate = r => r.Tags.Contains("ibmmq")
             });
           });

      var server = new TestServer(webHostBuilder);

      var response = await server.CreateRequest("/health")
          .GetAsync();

      response.StatusCode
          .Should().Be(HttpStatusCode.OK);
    }

    [SkipOnAppVeyor]
    public async Task be_unhealthy_if_ibmmq_is_unavailable()
    {
      Hashtable properties = new Hashtable();
      properties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
      properties.Add(MQC.CHANNEL_PROPERTY, channel);
      properties.Add(MQC.CONNECTION_NAME_PROPERTY, wrongHostName);
      properties.Add(MQC.USER_ID_PROPERTY, user);
      properties.Add(MQC.PASSWORD_PROPERTY, password);

      var webHostBuilder = new WebHostBuilder()
          .UseStartup<DefaultStartup>()
          .ConfigureServices(services =>
          {
            services.AddHealthChecks()
                  .AddIbmMQ(qManager, properties, tags: new string[] { "ibmmq" });
          })
          .Configure(app =>
          {
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
              Predicate = r => r.Tags.Contains("ibmmq")
            });
          });

      var server = new TestServer(webHostBuilder);

      var response = await server.CreateRequest("/health")
          .GetAsync();

      response.StatusCode
          .Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [SkipOnAppVeyor]
    public async Task be_unhealthy_if_ibmmq_managed_is_unavailable()
    {
      var webHostBuilder = new WebHostBuilder()
          .UseStartup<DefaultStartup>()
          .ConfigureServices(services =>
          {
            services.AddHealthChecks()
                  .AddIbmMQManagedConnection(qManager, channel, wrongHostName, user, password, tags: new string[] { "ibmmq" });
          })
          .Configure(app =>
          {
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
              Predicate = r => r.Tags.Contains("ibmmq")
            });
          });

      var server = new TestServer(webHostBuilder);

      var response = await server.CreateRequest("/health")
          .GetAsync();

      response.StatusCode
          .Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [SkipOnAppVeyor]
    public async Task be_healthy_if_ibmmq_managed_is_available()
    {
      var webHostBuilder = new WebHostBuilder()
           .UseStartup<DefaultStartup>()
           .ConfigureServices(services =>
           {
             services.AddHealthChecks().
                AddIbmMQManagedConnection(qManager, channel, hostName, user, password, tags: new string[] { "ibmmq" });
           })
           .Configure(app =>
           {
             app.UseHealthChecks("/health", new HealthCheckOptions()
             {
               Predicate = r => r.Tags.Contains("ibmmq")
             });
           });

      var server = new TestServer(webHostBuilder);

      var response = await server.CreateRequest("/health")
          .GetAsync();

      response.StatusCode
          .Should().Be(HttpStatusCode.OK);
    }
  }
}
