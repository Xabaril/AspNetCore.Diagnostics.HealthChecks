using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
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
        private readonly Settings _settings;

        public UIApiEndpointMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory, IOptions<Settings> settings)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
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
                    //Temporal query bifurcation until sqlite provider bug is fixed
                    //https://github.com/dotnet/efcore/issues/19178
                    if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
                    {
                        var entity = await db.Executions
                       .Include(le => le.Entries)
                       .Select(e => new
                       {
                           Execution = e,
                           History = db.HealthCheckExecutionHistories
                                   .Where(eh => EF.Property<int>(eh, "HealthCheckExecutionId") == e.Id)
                                   .OrderByDescending(eh => eh.On)
                                   .Take(_settings.MaximumExecutionHistoriesPerEndpoint)
                                   .ToList()
                       })
                       .Where(le => le.Execution.Name == item.Name)
                       .AsNoTracking()
                       .SingleOrDefaultAsync();

                        if (entity != null)
                        {
                            entity.Execution.History = entity.History;
                            healthChecksExecutions.Add(entity.Execution);
                        }
                    }
                    else
                    {
                        var execution = await db.Executions
                                    .Include(le => le.Entries)
                                    .Where(le => le.Name == item.Name)
                                    .AsNoTracking()
                                    .SingleOrDefaultAsync();

                        execution.History = await db.HealthCheckExecutionHistories
                                        .Where(eh => EF.Property<int>(eh, "HealthCheckExecutionId") == execution.Id)
                                        .OrderByDescending(eh => eh.On)
                                        .Take(_settings.MaximumExecutionHistoriesPerEndpoint)
                                        .ToListAsync();

                        if(execution != null)
                        {
                            healthChecksExecutions.Add(execution);
                        }
                    }

                }

                var responseContent = JsonConvert.SerializeObject(healthChecksExecutions, _jsonSerializationSettings);
                context.Response.ContentType = Keys.DEFAULT_RESPONSE_CONTENT_TYPE;

                await context.Response.WriteAsync(responseContent);
            }
        }
    }
}
