using System.Collections.Concurrent;
using Azure.Core;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.Management.DigitalTwins;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Rest;

namespace HealthChecks.AzureDigitalTwin
{
    public abstract class AzureDigitalTwinHealthCheck
    {
        protected const string MANAGEMENT_AZURE_URL = "https://management.azure.com";
        internal string ClientConnectionKey
        {
            get
            {
                string? hash = ClientId
                    ?? ServiceClientCredentials?.GetHashCode().ToString()
                    ?? TokenCredential?.GetHashCode().ToString();

                return hash
                    ?? throw new ArgumentNullException(nameof(ClientConnectionKey));
            }
        }

        protected static readonly ConcurrentDictionary<string, AzureDigitalTwinsManagementClient> ManagementClientConnections = new();
        protected static readonly ConcurrentDictionary<string, DigitalTwinsClient> DigitalTwinClientConnections = new();

        protected readonly string? ClientId;
        protected readonly string? ClientSecret;
        protected readonly string? TenantId;

        protected readonly ServiceClientCredentials? ServiceClientCredentials;
        protected readonly TokenCredential? TokenCredential;

        public AzureDigitalTwinHealthCheck(string clientId, string clientSecret, string tenantId)
        {
            ClientId = (!string.IsNullOrEmpty(clientId)) ? clientId : throw new ArgumentNullException(nameof(clientId));
            ClientSecret = (!string.IsNullOrEmpty(clientSecret)) ? clientSecret : throw new ArgumentNullException(nameof(clientSecret));
            TenantId = (!string.IsNullOrEmpty(tenantId)) ? tenantId : throw new ArgumentNullException(nameof(tenantId));
        }

        public AzureDigitalTwinHealthCheck(ServiceClientCredentials serviceClientCredentials)
        {
            ServiceClientCredentials = serviceClientCredentials ?? throw new ArgumentNullException(nameof(serviceClientCredentials));
        }

        public AzureDigitalTwinHealthCheck(TokenCredential tokenCredential)
        {
            TokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));
        }

        protected AzureDigitalTwinsManagementClient CreateManagementClient()
        {
            var credential = ServiceClientCredentials
                ?? new AzureCredentialsFactory().FromServicePrincipal(ClientId, ClientSecret, TenantId, AzureEnvironment.AzureGlobalCloud);
            return new AzureDigitalTwinsManagementClient(new Uri(MANAGEMENT_AZURE_URL), credential);
        }

        protected DigitalTwinsClient CreateDigitalTwinClient(string hostName)
        {
            var credential = TokenCredential
                ?? new ClientSecretCredential(TenantId, ClientId, ClientSecret);
            return new DigitalTwinsClient(new Uri(hostName), credential);
        }
    }
}
