using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthChecks.UI.Middleware
{
    internal class UIApiEndpointMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerSettings _jsonSerializationSettings;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UIApiEndpointMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
            _jsonSerializationSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter() },
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
        }
        public async Task InvokeAsync(HttpContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            using (var db = scope.ServiceProvider.GetService<HealthChecksDb>())
            {
                var healthChecks = await db.Configurations
                      .ToListAsync();

                var healthChecksExecutions = new List<HealthCheckExecution>();

                foreach (var item in healthChecks.OrderBy(h => h.Id))
                {
                    var execution = await db.Executions
                        .Include(le => le.History)
                        .Include(le => le.Entries)
                        .Where(le => le.Name == item.Name)
                        .AsNoTracking()
                        .SingleOrDefaultAsync();

                    if (execution != null)
                    {
                        healthChecksExecutions.Add(execution);
                    }
                }

                var responseContent = JsonConvert.SerializeObject(healthChecksExecutions, _jsonSerializationSettings);
                context.Response.ContentType = Keys.DEFAULT_RESPONSE_CONTENT_TYPE;

                await context.Response.WriteAsync(responseContent);
            }
        }
    }
}
