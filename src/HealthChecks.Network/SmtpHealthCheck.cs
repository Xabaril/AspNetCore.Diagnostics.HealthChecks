using HealthChecks.Network.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network
{
    public class SmtpHealthCheck 
        : IHealthCheck
    {
        private readonly SmtpHealthCheckOptions _options;
        private readonly ILogger<SmtpHealthCheck> _logger;

        public SmtpHealthCheck(SmtpHealthCheckOptions options, ILogger<SmtpHealthCheck> logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(SmtpHealthCheck)} is checking SMTP connections.");

                using (var smtpConnection = new SmtpConnection(_options))
                {
                    if (await smtpConnection.ConnectAsync())
                    {
                        if (_options.AccountOptions.Login)
                        {
                            var (user, password) = _options.AccountOptions.Account;

                            if (!await smtpConnection.AuthenticateAsync(user, password))
                            {
                                _logger?.LogWarning($"The {nameof(SmtpHealthCheck)} check fail with invalid login to smtp server {_options.Host}:{_options.Port} with configured credentials.");

                                return HealthCheckResult.Failed($"Error login to smtp server{_options.Host}:{_options.Port} with configured credentials");
                            }
                        }

                        _logger?.LogInformation($"The {nameof(SmtpHealthCheck)} check success.");

                        return HealthCheckResult.Passed();
                    }
                    else
                    {
                        _logger?.LogWarning($"The {nameof(SmtpHealthCheck)} check fail for connecting to smtp server {_options.Host}:{_options.Port} - SSL : {_options.ConnectionType}.");

                        return HealthCheckResult.Failed($"Could not connect to smtp server {_options.Host}:{_options.Port} - SSL : {_options.ConnectionType}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(SmtpHealthCheck)} check fail with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
