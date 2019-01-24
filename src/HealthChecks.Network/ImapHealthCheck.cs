using HealthChecks.Network.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network
{
    public class ImapHealthCheck
        : IHealthCheck
    {
        private readonly ImapHealthCheckOptions _options;
        private readonly TimeSpan _timeout;

        public ImapHealthCheck(ImapHealthCheckOptions options, TimeSpan timeout)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _timeout = timeout;

            if (string.IsNullOrEmpty(_options.Host))
            {
                throw new ArgumentNullException(nameof(_options.Host));
            }

            if (_options.Port == default)
            {
                throw new ArgumentNullException(nameof(_options.Port));
            }
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    using (var imapConnection = new ImapConnection(_options))
                    {

                        if (await imapConnection.ConnectAsync(timeoutCancellationTokenSource.Token))
                        {
                            if (_options.AccountOptions.Login)
                            {
                                return await ExecuteAuthenticatedUserActions(imapConnection, context, timeoutCancellationTokenSource.Token);
                            }
                        }
                        else
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Connection to server {_options.Host} has failed - SSL Enabled : {_options.ConnectionType}");
                        }

                        return HealthCheckResult.Healthy();
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

        private async Task<HealthCheckResult> ExecuteAuthenticatedUserActions(ImapConnection imapConnection, HealthCheckContext context, CancellationToken cancellationToken)
        {
            var (User, Password) = _options.AccountOptions.Account;

            if (await imapConnection.AuthenticateAsync(User, Password, cancellationToken))
            {
                if (_options.FolderOptions.CheckFolder
                    && !await imapConnection.SelectFolder(_options.FolderOptions.FolderName, cancellationToken))
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Folder {_options.FolderOptions.FolderName} check failed.");
                }

                return HealthCheckResult.Healthy();
            }
            else
            {
                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Login on server {_options.Host} failed with configured user");
            }
        }
    }
}
