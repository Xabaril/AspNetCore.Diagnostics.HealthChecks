using k8s;
using Microsoft.Extensions.DependencyInjection;
using System;
using HealthChecks.UI.K8s.Operator.Controller;
using HealthChecks.UI.K8s.Operator.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using HealthChecks.UI.K8s.Operator.Diagnostics;
using HealthChecks.UI.K8s.Operator.Operator;

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

                   Log.Logger.Information("Starting Kubernetes client using host: {host}", config.Host);

                   return new Kubernetes(config);
               })
                .AddHttpClient()
                .AddTransient<IHealthChecksController, HealthChecksController>()
                .AddSingleton<OperatorDiagnostics>()
                .AddSingleton<DeploymentHandler>()
                .AddSingleton<ServiceHandler>()
                .AddSingleton<SecretHandler>()
                .AddSingleton<ConfigMaphandler>()
                .AddSingleton<NotificationHandler>()
                .AddSingleton<NamespacedServiceWatcher>()
                .AddSingleton<ClusterServiceWatcher>();

            }).ConfigureLogging((context, builder) =>
            {
                var logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.WithProperty("Application", nameof(K8sOperator))
                    .Enrich.FromLogContext()
                    .WriteTo.ColoredConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception:lj}")
                    .CreateLogger();

                Log.Logger = logger;

                builder.ClearProviders();
                builder.AddSerilog(logger, dispose: true);
            });
    }
}

public class K8sOperator { } //Dummy class for logging

