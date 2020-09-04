using HealthChecks.Network.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Network.Extensions;

namespace HealthChecks.Network
{
    public class ImapHealthCheck
        : IHealthCheck
    {
        private readonly ImapHealthCheckOptions _options;
        public ImapHealthCheck(ImapHealthCheckOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

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
            try
            {
                using (var imapConnection = new ImapConnection(_options))
                {
                    if (await imapConnection.ConnectAsync().WithCancellationTokenAsync(cancellationToken))
                    {
                        if (_options.AccountOptions.Login)
                        {
                            return await ExecuteAuthenticatedUserActions(context, imapConnection);
                        }
                    }
                    else
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Connection to server {_options.Host} has failed - SSL Enabled : {_options.ConnectionType}");
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
        private async Task<HealthCheckResult> ExecuteAuthenticatedUserActions(HealthCheckContext context, ImapConnection imapConnection)
        {
            var (User, Password) = _options.AccountOptions.Account;

            if (await imapConnection.AuthenticateAsync(User, Password))
            {
                if (_options.FolderOptions.CheckFolder
                    && !await imapConnection.SelectFolder(_options.FolderOptions.FolderName))
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
