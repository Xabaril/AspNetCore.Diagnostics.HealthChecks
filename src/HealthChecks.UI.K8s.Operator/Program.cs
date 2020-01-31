using k8s;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Controller;
using HealthChecks.UI.K8s.Operator.Handlers;


namespace HealthChecks.UI.K8s.Operator
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = InitializeProvider();

            var @operator = provider.GetRequiredService<IKubernetesOperator>();
            
            var cancelTokenSource = new CancellationTokenSource();

            await @operator.RunAsync(cancelTokenSource.Token);

            var reset = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (s, a) =>
            {
                cancelTokenSource.Cancel();              
                @operator.Dispose();
                Console.WriteLine("Healthchecks Operator is shutting down...");
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

