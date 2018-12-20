using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace HealthChecks.UI.Core.Discovery.K8S.Extensions
{
    public static class KubernetesIHttpClientBuilderExtensions
    {
        /// <summary>
        /// Some cloud services like Azure AKS use self-signed certificates not valid for httpclient.
        /// With this method we allow invalid certificates
        /// </summary>
        public static IHttpClientBuilder ConfigureKubernetesMessageHandler(this IHttpClientBuilder builder)
        {
            return builder.ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler
            {                
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
        }
    }
}
