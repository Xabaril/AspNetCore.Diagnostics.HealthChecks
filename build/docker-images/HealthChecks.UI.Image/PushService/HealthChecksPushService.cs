using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthChecks.UI.Image.PushService
{
    internal class HealthChecksPushService
    {
        private readonly HealthChecksDb _db;

        public HealthChecksPushService(HealthChecksDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
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

                Console.WriteLine($"[Push] New service added: {name} with uri: {uri}");
            }

        }

        public async Task RemoveAsync(string name)
        {
            var endpoint = await Get(name);

            if (endpoint != null)
            {
                _db.Configurations.Remove(endpoint);
                await _db.SaveChangesAsync();

                Console.WriteLine($"[Push] Service removed: {name}");
            }
        }

        public async Task UpdateAsync(string name, string uri)
        {
            var endpoint = await Get(name);
            endpoint.Uri = uri;
            _db.Configurations.Update(endpoint);
            await _db.SaveChangesAsync();
            Console.WriteLine($"[Push] Service updated: {name} with uri {uri}");
        }

        private Task<HealthCheckConfiguration> Get(string name)
        {
            return _db.Configurations.FirstOrDefaultAsync(c => c.Name == name);
        }
    }
}
