using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.Core.Discovery
{
    internal class DiscoveryRegistryService : IDiscoveryRegistryService
    {
        private readonly ILogger<DiscoveryRegistryService> _logger;
        private readonly HealthChecksDb _db;
        private readonly HttpClient _httpClient;

        public DiscoveryRegistryService(ILogger<DiscoveryRegistryService> logger, HealthChecksDb db, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = db ?? throw new ArgumentNullException(nameof(db));

            _httpClient = httpClientFactory.CreateClient(Keys.DISCOVERY_SERVICE_HTTP_CLIENT_NAME);
        }

        public async Task RegisterService(string service, string name, Uri uri, CancellationToken cancellationToken = default)
        {
            // Check if we should register
            if (await IsLivenessRegistered(uri, cancellationToken))
            {
                _logger.LogDebug("Skipping service {Name} at {Uri}, already registered", name, uri);
                return;
            }

            // Register it
            try
            {
                if (await IsValidHealthChecksStatusCode(uri, cancellationToken))
                {
                    await RegisterDiscoveredLiveness(service, name, uri, cancellationToken);

                    _logger.LogInformation("Registered discovered liveness on {Address} with name {name}", uri, name);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error discovering service {Name} at {Uri}", name, uri);
            }
        }

        async Task<bool> IsLivenessRegistered(Uri uri, CancellationToken cancellationToken)
        {
            string strUri = uri.ToString();
            return await _db.Configurations.AnyAsync(lc => lc.Uri == strUri, cancellationToken);
        }

        async Task<bool> IsValidHealthChecksStatusCode(Uri uri, CancellationToken cancellationToken)
        {
            using (var response = await _httpClient.GetAsync(uri, cancellationToken))
                return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable;
        }
        async Task RegisterDiscoveredLiveness(string service, string name, Uri uri, CancellationToken cancellationToken)
        {
            _db.Configurations.Add(new HealthCheckConfiguration
            {
                Name = name,
                Uri = uri.ToString(),
                DiscoveryService = service
            });

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}