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

        private ImapConnection _imapConnection = null;

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
                _imapConnection = new ImapConnection(_options);

                if (await _imapConnection.ConnectAsync())
                {
                    if (_options.AccountOptions.Login)
                    {
                        return await ExecuteAuthenticatedUserActions();
                    }
                }
                else
                {
                    return HealthCheckResult.Failed($"Connection to server {_options.Host} has failed - SSL Enabled : {_options.ConnectionType}");
                }

                return HealthCheckResult.Passed();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Failed(exception:ex);
            }
            finally
            {
                _imapConnection.Dispose();
            }
        }

        private async Task<HealthCheckResult> ExecuteAuthenticatedUserActions()
        {
            var (User, Password) = _options.AccountOptions.Account;

            if (await _imapConnection.AuthenticateAsync(User, Password))
            {
                if (_options.FolderOptions.CheckFolder 
                    && ! await _imapConnection.SelectFolder(_options.FolderOptions.FolderName))
                {
                    return HealthCheckResult.Failed($"Folder {_options.FolderOptions.FolderName} check failed.");
                }
                
                return HealthCheckResult.Passed();
            }
            else
            {
                return HealthCheckResult.Failed($"Login on server {_options.Host} failed with configured user");
            }
        }
    }
}
