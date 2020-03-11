using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthChecks.UI.Image.PushService
{
    internal class HealthChecksPushService
    {
        private readonly HealthChecksDb _db;
        private readonly ILogger<HealthChecksPushService> _logger;

        public HealthChecksPushService(HealthChecksDb db, ILogger<HealthChecksPushService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentException(nameof(logger));
        }
        public async Task AddAsync(string name, string uri)
        {
            if ((await Get(name)) == null)
            {
                await _db.Configurations.AddAsync(new HealthCheckConfiguration
                {
                    Name = name,
                    Uri = uri,
                    DiscoveryService = "kubernetes"
                });

                await _db.SaveChangesAsync();

                _logger.LogInformation("[Push] New service added: {name} with uri: {uri}", name, uri);
            }
        }

        public async Task RemoveAsync(string name)
        {
            var endpoint = await Get(name);

            if (endpoint != null)
            {
                _db.Configurations.Remove(endpoint);
                await _db.SaveChangesAsync();

                _logger.LogInformation("[Push] Service removed: {name}", name);
            }
        }

        public async Task UpdateAsync(string name, string uri)
        {
            var endpoint = await Get(name);

            if (endpoint != null)
            {
                endpoint.Uri = uri;
                _db.Configurations.Update(endpoint);
                await _db.SaveChangesAsync();

                _logger.LogInformation("[Push] Service updated: {name} with uri {uri}", name, uri);
            }
        }

        private Task<HealthCheckConfiguration> Get(string name)
        {
            return _db.Configurations.FirstOrDefaultAsync(c => c.Name == name);
        }
    }
}
