using HealthChecks.Network.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network
{
    public class ImapHealthCheck 
        : IHealthCheck
    {
        private readonly ImapHealthCheckOptions _options;
        private readonly ILogger<ImapHealthCheck> _logger;

        private ImapConnection _imapConnection = null;

        public ImapHealthCheck(ImapHealthCheckOptions options, ILogger<ImapHealthCheck> logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;

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
                _logger?.LogInformation($"{nameof(ImapHealthCheck)} is checking IMAP entries.");

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
                    _logger?.LogWarning($"{nameof(ImapHealthCheck)} fail connect to server {_options.Host}- SSL Enabled : {_options.ConnectionType}.");

                    return HealthCheckResult.Failed($"Connection to server {_options.Host} has failed - SSL Enabled : {_options.ConnectionType}");
                }

                _logger?.LogInformation($"The {nameof(ImapHealthCheck)} check success.");

                return HealthCheckResult.Passed();

            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(ImapHealthCheck)} check fail with the exception {ex.ToString()}.");

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
                    _logger?.LogWarning($"{nameof(ImapHealthCheck)} fail connect to server {_options.Host}- SSL Enabled : {_options.ConnectionType} and open folder {_options.FolderOptions.FolderName}.");

                    return HealthCheckResult.Failed($"Folder {_options.FolderOptions.FolderName} check failed.");
                }
                
                _logger?.LogInformation($"The {nameof(ImapHealthCheck)} check success.");

                return HealthCheckResult.Passed();
            }
            else
            {
                _logger?.LogWarning($"{nameof(ImapHealthCheck)} fail connect to server {_options.Host}- SSL Enabled : {_options.ConnectionType}.");

                return HealthCheckResult.Failed($"Login on server {_options.Host} failed with configured user");
            }
        }
    }
}
