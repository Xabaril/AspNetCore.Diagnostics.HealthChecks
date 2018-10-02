using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Middleware
{
    class UIApiEndpointMiddleware
    {
        private readonly RequestDelegate _next;

        public UIApiEndpointMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<HealthChecksDb>();

                var cancellationToken = new CancellationToken();

                var registeredLiveness = await db.Configurations
                    .ToListAsync(cancellationToken);

                var livenessExecutions = new List<HealthCheckExecution>();

                foreach (var item in registeredLiveness)
                {
                    var execution = await db.Executions
                        .Include(le => le.History)
                        .Where(le => le.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase))
                        .SingleOrDefaultAsync(cancellationToken);

                    if (execution != null)
                    {
                        livenessExecutions.Add(execution);
                    }
                }

                var responseContent = JsonConvert.SerializeObject(livenessExecutions, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

                context.Response.ContentType = Keys.DEFAULT_RESPONSE_CONTENT_TYPE;

                await context.Response.WriteAsync(responseContent);
            }
        }
    }
}
