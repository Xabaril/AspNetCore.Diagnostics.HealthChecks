using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task AddAsync(string name, string? group, string uri)
        {
            if ((await Get(name, group)) == null)
            {
                await _db.Configurations.AddAsync(new HealthCheckConfiguration
                {
                    Name = name,
                    Group = group,
                    Uri = uri,
                    DiscoveryService = "kubernetes"
                });

                await _db.SaveChangesAsync();

                _logger.LogInformation("[Push] New service added: {name} ({group}) with uri: {uri}", name, group, uri);
            }
        }

        public async Task RemoveAsync(string name, string? group)
        {
            var endpoint = await Get(name, group);

            if (endpoint != null)
            {
                _db.Configurations.Remove(endpoint);
                await _db.SaveChangesAsync();

                _logger.LogInformation("[Push] Service removed: {name} ({group})", name, group);
            }
        }

        public async Task UpdateAsync(string name, string? group, string uri)
        {
            var endpoint = await Get(name, group);

            if (endpoint != null)
            {
                endpoint.Uri = uri;
                _db.Configurations.Update(endpoint);
                await _db.SaveChangesAsync();

                _logger.LogInformation("[Push] Service updated: {name} ({group}) with uri {uri}", name, group, uri);
            }
        }

        private Task<HealthCheckConfiguration?> Get(string name, string? group)
        {
            return _db.Configurations.FirstOrDefaultAsync(c => c.Name == name && c.Group == group);
        }
    }
}
