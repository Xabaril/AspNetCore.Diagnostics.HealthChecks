using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
#if !NET5_0_OR_GREATER
using HealthChecks.Network.Extensions;
#endif
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Network;

public class SslHealthCheck : IHealthCheck
{
    private readonly SslHealthCheckOptions _options;

    public SslHealthCheck(SslHealthCheckOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string>? errorList = null;
            foreach (var (host, port, checkLeftDays) in _options.ConfiguredHosts)
            {
                using var tcpClient = new TcpClient(_options.AddressFamily);
#if NET5_0_OR_GREATER
                await tcpClient.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
#else
                await tcpClient.ConnectAsync(host, port).WithCancellationTokenAsync(cancellationToken).ConfigureAwait(false);
#endif
                if (!tcpClient.Connected)
                {
                    (errorList ??= new()).Add($"Connection to host {host}:{port} failed");
                    if (!_options.CheckAllHosts)
                    {
                        break;
                    }
                    continue;
                }

                using var certificate = await GetSslCertificateAsync(tcpClient, host).ConfigureAwait(false);

                if (certificate is null || !certificate.Verify())
                {
                    (errorList ??= new()).Add($"Ssl certificate not present or not valid for {host}:{port}");
                    if (!_options.CheckAllHosts)
                    {
                        break;
                    }
                    continue;
                }

                if (certificate.NotAfter.Subtract(DateTime.Now).TotalDays <= checkLeftDays)
                {
                    (errorList ??= new()).Add($"Ssl certificate for {host}:{port} is about to expire in {checkLeftDays} days");
                    if (!_options.CheckAllHosts)
                    {
                        break;
                    }
                }
            }

            return errorList.GetHealthState(context);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private async Task<X509Certificate2?> GetSslCertificateAsync(TcpClient client, string host)
    {
        // https://github.com/DotNetAnalyzers/IDisposableAnalyzers/issues/513
#pragma warning disable IDISP004 // Don't ignore created IDisposable
        using var ssl = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback((sender, cert, ca, sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None), null);
#pragma warning restore IDISP004 // Don't ignore created IDisposable

        try
        {
            await ssl.AuthenticateAsClientAsync(host).ConfigureAwait(false);
            var cert = ssl.RemoteCertificate;
            return cert == null ? null : new X509Certificate2(cert);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
