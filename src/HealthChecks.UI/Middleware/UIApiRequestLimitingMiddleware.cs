using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace HealthChecks.UI.Middleware
{
    internal sealed class UIApiRequestLimitingMiddleware : IDisposable
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<Settings> _settings;
        private readonly ILogger<UIApiEndpointMiddleware> _logger;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public UIApiRequestLimitingMiddleware(RequestDelegate next, IOptions<Settings> settings, ILogger<UIApiEndpointMiddleware> logger)
        {
            _next = Guard.ThrowIfNull(next);
            _settings = Guard.ThrowIfNull(settings);
            _logger = Guard.ThrowIfNull(logger);

            var maxActiveRequests = _settings.Value.ApiMaxActiveRequests;

            if (maxActiveRequests <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxActiveRequests));
            }

            _semaphore = new SemaphoreSlim(maxActiveRequests, maxActiveRequests);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!await _semaphore.WaitAsync(TimeSpan.Zero).ConfigureAwait(false))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return;
            }

            try
            {
                _logger.LogDebug("Executing api middleware for client {client}, remaining slots: {slots}", context.Connection.RemoteIpAddress, _semaphore.CurrentCount);

                await _next(context).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _semaphore.Dispose();
            _disposed = true;
        }
    }
}
