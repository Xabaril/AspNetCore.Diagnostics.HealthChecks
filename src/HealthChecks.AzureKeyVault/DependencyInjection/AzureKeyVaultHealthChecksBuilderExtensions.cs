using Azure.Core;
using HealthChecks.AzureKeyVault;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureKeyVaultHealthChecksBuilderExtensions
    {
        const string KEYVAULT_NAME = "azurekeyvault";

        /// <summary>
        /// Add a health check for Azure Key Vault. Default behaviour is using Managed Service Identity, to use Client Secrets call UseClientSecrets in setup action
        /// </summary>
        /// <param name="keyVaultServiceUri">The AzureKeyVault service uri.</param>
        /// <param name="credential">The TokenCredentail to use, you can use Azure.Identity with DefaultAzureCredential or other kind of TokenCredential,you can read more on <see href="https://github.com/Azure/azure-sdk-for-net/blob/Azure.Identity_1.2.2/sdk/identity/Azure.Identity/README.md"/>. </param>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup"> Setup action to configure Azure Key Vault options.</param>    
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurekeyvault' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddAzureKeyVault(this IHealthChecksBuilder builder, Uri keyVaultServiceUri, TokenCredential credential, Action<AzureKeyVaultOptions> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var options = new AzureKeyVaultOptions();
            setup?.Invoke(options);

            if (keyVaultServiceUri == null) throw new ArgumentNullException(nameof(keyVaultServiceUri));
            if (credential == null) throw new ArgumentNullException(nameof(credential));

            return builder.Add(new HealthCheckRegistration(
               name ?? KEYVAULT_NAME,
               sp => new AzureKeyVaultHealthCheck(keyVaultServiceUri, credential, options),
               failureStatus,
               tags,
               timeout));
        }
    }
}
