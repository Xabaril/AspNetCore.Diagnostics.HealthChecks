using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Utilities.Net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IPAddress = System.Net.IPAddress;
namespace HealthChecks.UI.Middleware
{
    internal class UIApiRequestLimitingMidleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<Settings> _settings;
        private readonly ILogger<UIApiEndpointMiddleware> _logger;
        private readonly SemaphoreSlim _semaphore;

        public UIApiRequestLimitingMidleware(RequestDelegate next, IOptions<Settings> settings, ILogger<UIApiEndpointMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var maxActiveRequests = _settings.Value.ApiMaxActiveRequests;

            if (maxActiveRequests <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxActiveRequests));
            }

            _semaphore = new SemaphoreSlim(maxActiveRequests, maxActiveRequests);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!await _semaphore.WaitAsync(TimeSpan.Zero))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return;
            }

            try
            {
                _logger.LogDebug("Executing api middleware for client {client}, remaining slots: {slots}",
                    context.Connection.RemoteIpAddress,
                    _semaphore.CurrentCount);

                await _next(context);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
