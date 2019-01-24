using HealthChecks.Network.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network
{
    public class SmtpHealthCheck
        : IHealthCheck
    {
        private readonly SmtpHealthCheckOptions _options;
        private readonly TimeSpan _timeout;

        public SmtpHealthCheck(SmtpHealthCheckOptions options, TimeSpan timeout)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    using (var smtpConnection = new SmtpConnection(_options))
                    {
                        if (await smtpConnection.ConnectAsync(timeoutCancellationTokenSource.Token))
                        {
                            if (_options.AccountOptions.Login)
                            {
                                var (user, password) = _options.AccountOptions.Account;

                                if (!await smtpConnection.AuthenticateAsync(user, password, timeoutCancellationTokenSource.Token))
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
                    if (timeoutCancellationTokenSource.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, "Timeout");
                    }
                    return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                }
            }
        }
    }
}
