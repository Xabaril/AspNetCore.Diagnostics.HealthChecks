using k8s;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Controller;
using HealthChecks.UI.K8s.Operator.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using HealthChecks.UI.K8s.Operator.Diagnostics;

namespace HealthChecks.UI.K8s.Operator
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var diagnostics = host.Services.GetRequiredService<OperatorDiagnostics>();

            try
            {
                host.Run();
            }
            catch (Exception exception)
            {
                diagnostics.OperatorThrow(exception);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<HealthChecksOperator>()
                .AddSingleton<IKubernetes>(sp =>
                {
                    var config = KubernetesClientConfiguration.IsInCluster() ?
                                   KubernetesClientConfiguration.InClusterConfig() :
                                   KubernetesClientConfiguration.BuildConfigFromConfigFile();

                    return new Kubernetes(config);
                })
                .AddTransient<IHealthChecksController, HealthChecksController>()
                .AddSingleton<OperatorDiagnostics>()
                .AddSingleton<DeploymentHandler>()
                .AddSingleton<ServiceHandler>()
                .AddSingleton<SecretHandler>()
                .AddSingleton<ConfigMaphandler>()
                .AddSingleton<HealthCheckServiceWatcher>();

            }).ConfigureLogging((context, builder) =>
            {
                var logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.WithProperty("Application", nameof(K8sOperator))
                    .Enrich.FromLogContext()
                    .WriteTo.ColoredConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception:lj}")
                    .CreateLogger();

                builder.ClearProviders();
                builder.AddSerilog(logger, dispose: true);
            });
    }
}

public class K8sOperator { } //Dummy class for logging

