using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace HealthChecks.Network
{
    public class SftpHealthCheck 
        : IHealthCheck
    {
        private readonly SftpHealthCheckOptions _options;
        private readonly ILogger<SftpHealthCheck> _logger;

        public SftpHealthCheck(SftpHealthCheckOptions options, ILogger<SftpHealthCheck> logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(SftpHealthCheck)} is checking SFTP connections.");

                foreach (var item in _options.ConfiguredHosts.Values)
                {
                    var connectionInfo = new ConnectionInfo(item.Host, item.UserName, item.AuthenticationMethods.ToArray());

                    using (var sftpClient = new SftpClient(connectionInfo))
                    {
                        sftpClient.Connect();

                        var connectionSuccess = sftpClient.IsConnected && sftpClient.ConnectionInfo.IsAuthenticated;

                        if (connectionSuccess)
                        {
                            if (item.FileCreationOptions.createFile)
                            {
                                using (var stream = new MemoryStream(new byte[] { 0x0 }, 0, 1))
                                {
                                    sftpClient.UploadFile(stream, item.FileCreationOptions.remoteFilePath);
                                }
                            }
                        }
                        else
                        {
                            _logger?.LogWarning($"The {nameof(SftpHealthCheck)} check fail for sftp host {item.Host}.");

                            return Task.FromResult(
                                HealthCheckResult.Failed($"Connection with sftp host {item.Host}:{item.Port} failed"));
                        }
                    }
                }

                _logger?.LogInformation($"The {nameof(SftpHealthCheck)} check success.");

                return Task.FromResult(
                    HealthCheckResult.Passed());

            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(SftpHealthCheck)} check fail with the exception {ex.ToString()}.");

                return Task.FromResult(
                    HealthCheckResult.Failed(exception:ex));
            }
        }
    }
}
