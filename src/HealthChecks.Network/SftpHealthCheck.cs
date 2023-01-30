using Microsoft.Extensions.Diagnostics.HealthChecks;
using Renci.SshNet;
using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace HealthChecks.Network
{
    public class SftpHealthCheck : IHealthCheck
    {
        private readonly SftpHealthCheckOptions _options;

        public SftpHealthCheck(SftpHealthCheckOptions options)
        {
            _options = Guard.ThrowIfNull(options);
        }

        /// <inheritdoc />
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var item in _options.ConfiguredHosts.Values)
                {
                    var connectionInfo = new ConnectionInfo(item.Host, item.Port, item.UserName, item.AuthenticationMethods.ToArray());

                    if (context.Registration.Timeout > TimeSpan.Zero)
                    {
                        connectionInfo.Timeout = context.Registration.Timeout;
                    }

                    using var sftpClient = new SftpClient(connectionInfo);

                    sftpClient.Connect();

                    bool connectionSuccess = sftpClient.IsConnected && sftpClient.ConnectionInfo.IsAuthenticated;
                    if (connectionSuccess)
                    {
                        if (item.FileCreationOptions.createFile)
                        {
                            using var stream = new MemoryStream(new byte[] { 0x0 }, 0, 1);
                            sftpClient.UploadFile(stream, item.FileCreationOptions.remoteFilePath);
                        }
                    }
                    else
                    {
                        return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: $"Connection with sftp host {item.Host}:{item.Port} failed."));
                    }
                }

                return HealthCheckResultTask.Healthy;
            }
            catch (Exception ex)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}
