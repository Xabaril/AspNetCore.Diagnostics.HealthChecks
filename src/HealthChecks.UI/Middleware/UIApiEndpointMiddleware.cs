using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace HealthChecks.UI.Middleware
{
    internal class UIApiEndpointMiddleware
    {
        private readonly JsonSerializerSettings _jsonSerializationSettings;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Settings _settings;

        public UIApiEndpointMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory, IOptions<Settings> settings)
        {
            _ = next;
            _serviceScopeFactory = serviceScopeFactory;
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _jsonSerializationSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter() },
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<HealthChecksDb>())
            {
                var healthChecks = await db.Configurations.ToListAsync();

                var groupedHealthCheckExecutions = new List<GroupedHealthCheckExecutions>();
                foreach (var group in healthChecks.GroupBy(check => check.Group).OrderBy(group => group.Key))
                {
                    var healthCheckExecutions = new List<HealthCheckExecution>(group.Count());

                    var lastStateChanged = new DateTime?();
                    var lastExecution = new DateTime?();
                    var status = UIHealthStatus.Healthy;
                    foreach (var item in group.OrderBy(item => item.Name))
                    {
                        var execution = await db.Executions
                                    .Include(le => le.Entries)
                                    .Where(le => le.Name == item.Name && le.Group == item.Group)
                                    .AsNoTracking()
                                    .SingleOrDefaultAsync();

                        if (execution != null)
                        {
                            execution.History = await db.HealthCheckExecutionHistories
                                .Where(eh => EF.Property<int>(eh, "HealthCheckExecutionId") == execution.Id)
                                .OrderByDescending(eh => eh.On)
                                .Take(_settings.MaximumExecutionHistoriesPerEndpoint)
                                .ToListAsync();

                            if (!lastStateChanged.HasValue || lastStateChanged.Value < execution.OnStateFrom)
                                lastStateChanged = execution.OnStateFrom;

                            if (!lastExecution.HasValue || lastExecution.Value < execution.LastExecuted)
                                lastExecution = execution.LastExecuted;

                            switch (status)
                            {
                                case UIHealthStatus.Healthy when execution.Status != UIHealthStatus.Healthy:
                                case UIHealthStatus.Degraded when execution.Status == UIHealthStatus.Unhealthy:
                                    status = execution.Status;
                                    break;
                            }

                            healthCheckExecutions.Add(execution);
                        }
                    }

                    var groupedHealthCheckExecution = new GroupedHealthCheckExecutions
                    {
                        Name = group.Key,
                        LastExecuted = lastExecution,
                        OnStateFrom = lastStateChanged,
                        Status = status,
                        Executions = healthCheckExecutions
                    };

                    groupedHealthCheckExecutions.Add(groupedHealthCheckExecution);
                }
                
                var responseContent = JsonConvert.SerializeObject(groupedHealthCheckExecutions, _jsonSerializationSettings);
                context.Response.ContentType = Keys.DEFAULT_RESPONSE_CONTENT_TYPE;

                await context.Response.WriteAsync(responseContent);
            }
        }
    }
}
