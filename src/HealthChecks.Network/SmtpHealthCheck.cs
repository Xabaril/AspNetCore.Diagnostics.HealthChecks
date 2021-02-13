using HealthChecks.Network.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Network.Extensions;

namespace HealthChecks.Network
{
    public class SmtpHealthCheck
        : IHealthCheck
    {
        private readonly SmtpHealthCheckOptions _options;
        public SmtpHealthCheck(SmtpHealthCheckOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var smtpConnection = new SmtpConnection(_options))
                {
                    if (await smtpConnection.ConnectAsync())
                    {
                        if (_options.AccountOptions.Login)
                        {
                            var (user, password) = _options.AccountOptions.Account;

                            if (!await smtpConnection.AuthenticateAsync(user, password).WithCancellationTokenAsync(cancellationToken))
                            {
                                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Error login to smtp server{_options.Host}:{_options.Port} with configured credentials");
                            }
                        }

                        return HealthCheckResult.Healthy();
                    }
                    else
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Could not connect to smtp server {_options.Host}:{_options.Port} - SSL : {_options.ConnectionType}");
                    }
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
