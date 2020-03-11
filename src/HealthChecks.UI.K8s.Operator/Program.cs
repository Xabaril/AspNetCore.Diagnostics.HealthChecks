using k8s;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Controller;
using HealthChecks.UI.K8s.Operator.Handlers;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator
{

    class Program
    {
        static void Main(string[] args)
        {
            var provider = InitializeProvider();

            var logger = provider.GetService<ILogger<K8sOperator>>();

            var @operator = provider.GetRequiredService<IKubernetesOperator>();

            var cancelTokenSource = new CancellationTokenSource();

            logger.LogInformation("Healthchecks Operator is starting...");

            _ = @operator.RunAsync(cancelTokenSource.Token);

            var reset = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (s, a) =>
            {
                logger.LogInformation("Healthchecks Operator is shutting down...");
                @operator.Dispose();
                cancelTokenSource.Cancel();

                reset.Set();
            };

            reset.Wait();
        }
        private static IKubernetes GetKubernetesClient()
        {
            var config = KubernetesClientConfiguration.IsInCluster() ?
                KubernetesClientConfiguration.InClusterConfig() :
                KubernetesClientConfiguration.BuildConfigFromConfigFile();

            return new Kubernetes(config);
        }

        private static IServiceProvider InitializeProvider()
        {
            var services = new ServiceCollection();
            services.AddLogging(config =>
            {
                config.AddConsole();
                config.AddFilter(typeof(Program).Namespace, LogLevel.Information);
                config.AddFilter("Microsoft", LogLevel.None);
            });
            services.AddSingleton(sp => GetKubernetesClient());
            services.AddTransient<IKubernetesOperator, HealthChecksOperator>();
            services.AddTransient<IHealthChecksController, HealthChecksController>();
            services.AddSingleton<DeploymentHandler>();
            services.AddSingleton<ServiceHandler>();
            services.AddSingleton<SecretHandler>();
            services.AddSingleton<HealthCheckServiceWatcher>();

            return services.BuildServiceProvider();
        }
    }
}

public class K8sOperator { } //Dummy class for logging

