//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Net.Http;
//using System.Text;
//using System.Threading;
//using FluentAssertions;
//using FunctionalTests.Base;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.TestHost;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
//using Xunit;

//namespace FunctionalTests.HealthChecks.Publisher.Prometheus
//{
//    [Collection("execution")]
//    public class prometheus_publisher_should : IDisposable
//    {
//        private const string PrometheusEndpoint = "http://localhost";
//        private const string JobName = "myjob";
//        private readonly TestServer _fakeEndpoint;
//        private readonly HttpClient _fakePrometheusGatewayClient;
//        private readonly AutoResetEvent _finishedTrigger;
//        private string _publishedResult;
//        private static readonly TimeSpan TimeoutForPublishingResults = TimeSpan.FromSeconds(15);

//        public prometheus_publisher_should()
//        {
//            _finishedTrigger = new AutoResetEvent(false);
//            _publishedResult = string.Empty;

//            _fakeEndpoint = new TestServer(new WebHostBuilder()
//                .Configure(app =>
//                {
//                    app.Run(async context =>
//                    {
//                        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
//                        {
//                            _publishedResult = await reader.ReadToEndAsync();
//                            Debug.WriteLine(_publishedResult);
//                            _finishedTrigger.Set();
//                        }
//                    });
//                }));
//            _fakePrometheusGatewayClient = _fakeEndpoint.CreateClient();
//        }

//        public void Dispose()
//        {
//            _finishedTrigger?.Dispose();
//            _fakePrometheusGatewayClient?.Dispose();
//            _fakeEndpoint?.Dispose();
//        }

//        [SkipOnAppVeyor]
//        public void publish_healthy_result_when_health_checks_are()
//        {
//            var sut = new TestServer(new WebHostBuilder()
//                .UseStartup<DefaultStartup>()
//                .ConfigureServices(services =>
//                {
//                    services.AddHealthChecks()
//                        .AddCheck("fake", check => HealthCheckResult.Healthy())
//                        .AddPrometheusGatewayPublisher(_fakePrometheusGatewayClient, PrometheusEndpoint, JobName);
//                }));

//            _finishedTrigger.WaitOne(TimeoutForPublishingResults);

//            _publishedResult.Should().ContainCheckAndResult("fake", HealthStatus.Healthy);
//        }

//        [SkipOnAppVeyor]
//        public void publish_unhealthy_result_when_health_checks_are()
//        {
//            var sut = new TestServer(new WebHostBuilder()
//                .UseStartup<DefaultStartup>()
//                .ConfigureServices(services =>
//                {
//                    services.AddHealthChecks()
//                        .AddCheck("unhealthy", check => HealthCheckResult.Unhealthy())
//                        .AddPrometheusGatewayPublisher(_fakePrometheusGatewayClient, PrometheusEndpoint, JobName);
//                }));

//            _finishedTrigger.WaitOne(TimeoutForPublishingResults);

//            _publishedResult.Should().ContainCheckAndResult("unhealthy", HealthStatus.Unhealthy);
//        }

//        [SkipOnAppVeyor]
//        public void publish_all_results_included_when_there_are_three_checks_with_different_results()
//        {
//            var sut = new TestServer(new WebHostBuilder()
//                .UseStartup<DefaultStartup>()
//                .ConfigureServices(services =>
//                {
//                    services.AddHealthChecks()
//                        .AddCheck("unhealthy", check => HealthCheckResult.Unhealthy())
//                        .AddCheck("healthy", check => HealthCheckResult.Healthy())
//                        .AddCheck("degraded", check => HealthCheckResult.Degraded())
//                        .AddPrometheusGatewayPublisher(_fakePrometheusGatewayClient, PrometheusEndpoint, JobName);
//                }));

//            _finishedTrigger.WaitOne(TimeoutForPublishingResults);

//            _publishedResult.Should().ContainCheckAndResult("unhealthy", HealthStatus.Unhealthy);
//            _publishedResult.Should().ContainCheckAndResult("healthy", HealthStatus.Healthy);
//            _publishedResult.Should().ContainCheckAndResult("degraded", HealthStatus.Degraded);
//        }
//    }
//}