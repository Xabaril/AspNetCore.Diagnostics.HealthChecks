using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        public SftpHealthCheck(SftpHealthCheckOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var item in _options.ConfiguredHosts.Values)
                {
                    var connectionInfo = new ConnectionInfo(item.Host, item.Port, item.UserName, item.AuthenticationMethods.ToArray());

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
                            return Task.FromResult(
                                new HealthCheckResult(context.Registration.FailureStatus, description: $"Connection with sftp host {item.Host}:{item.Port} failed."));
                        }
                    }
                }

                return Task.FromResult(
                    HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}
