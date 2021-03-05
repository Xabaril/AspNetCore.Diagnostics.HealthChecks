using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Network.Extensions;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace HealthChecks.Network
{
    public class SslHealthCheck
        : IHealthCheck
    {
        private readonly SslHealthCheckOptions _options;
        public SslHealthCheck(SslHealthCheckOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var (host, port, checkLeftDays) in _options.ConfiguredHosts)
                {
                    using (var tcpClient = new TcpClient())
                    {
                        await tcpClient.ConnectAsync(host, port).WithCancellationTokenAsync(cancellationToken);

                        if (!tcpClient.Connected)
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Connection to host {host}:{port} failed");
                        }

                        var crt = await GetSslCertificate(tcpClient, host);

                        if (crt is null || !crt.Verify())
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Ssl certificate not present or not valid for {host}:{port}");
                        }

                        if (crt.NotAfter.Subtract(DateTime.Now).TotalDays <= checkLeftDays)
                        {
                            return HealthCheckResult.Degraded(description: $"Ssl certificate for {host}:{port} is about to expire in {checkLeftDays} days");
                        }

                        return HealthCheckResult.Healthy();
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private async Task<X509Certificate2> GetSslCertificate(TcpClient client, string host)
        {
                SslStream ssl = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback((sender, cert, ca, sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None), null);

                try
                {
                    await ssl.AuthenticateAsClientAsync(host);
                    var cert = new X509Certificate2(ssl.RemoteCertificate);
                    ssl.Close();
                    return cert;
                }
                catch (Exception)
                {
                    ssl.Close();
                    return null;
                }
        }

    }
}
