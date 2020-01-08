using HealthChecks.UI.K8s.Controller.Controller;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;


namespace HealthChecks.UI.K8s.Controller
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = InitializeProvider();

            var @operator = provider.GetRequiredService<IKubernetesOperator>();
            
            var cancelTokenSource = new CancellationTokenSource();

            await @operator.RunAsync(cancelTokenSource.Token);

            var reset = new ManualResetEventSlim();
            Console.CancelKeyPress += (s, a) => reset.Set();

            reset.Wait();
        }

        private static IServiceProvider InitializeProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient(sp => GetKubernetesClient());
            services.AddTransient(sp => new HealthChecksController());
           
            return services.BuildServiceProvider();
        }

        private static IKubernetes GetKubernetesClient()
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            return new Kubernetes(config);
        }
    }
}

