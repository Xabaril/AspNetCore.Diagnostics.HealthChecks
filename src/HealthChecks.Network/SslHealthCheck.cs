using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Network.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
                    using (var tcpClient = new TcpClient(_options.AddressFamily))
                    {
                        await tcpClient.ConnectAsync(host, port).WithCancellationTokenAsync(cancellationToken);

                        if (!tcpClient.Connected)
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Connection to host {host}:{port} failed");
                        }

                        var certificate = await GetSslCertificate(tcpClient, host);

                        if (certificate is null || !certificate.Verify())
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Ssl certificate not present or not valid for {host}:{port}");
                        }

                        if (certificate.NotAfter.Subtract(DateTime.Now).TotalDays <= checkLeftDays)
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Ssl certificate for {host}:{port} is about to expire in {checkLeftDays} days");
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
            var ssl = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback((sender, cert, ca, sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None), null);

            try
            {
                await ssl.AuthenticateAsClientAsync(host);
                return new X509Certificate2(ssl.RemoteCertificate);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                ssl.Close();
            }
        }

    }
}
