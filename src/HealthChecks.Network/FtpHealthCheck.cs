using System.Net;
using HealthChecks.Network.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Network;

public class FtpHealthCheck : IHealthCheck
{
    private readonly FtpHealthCheckOptions _options;

    public FtpHealthCheck(FtpHealthCheckOptions options)
    {
        _options = options ?? throw new ArgumentException(nameof(options));
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            HealthCheckErrorList? errorList = null;
            foreach (var (host, createFile, credentials) in _options.Hosts.Values)
            {
                var ftpRequest = CreateFtpWebRequest(host, createFile, credentials);

                using var ftpResponse = (FtpWebResponse)await ftpRequest.GetResponseAsync().WithCancellationTokenAsync(cancellationToken).ConfigureAwait(false);

                if (ftpResponse.StatusCode != FtpStatusCode.PathnameCreated && ftpResponse.StatusCode != FtpStatusCode.ClosingData)
                {
                    (errorList ??= new()).Add($"Error connecting to ftp host {host} with exit code {ftpResponse.StatusCode}");
                    if (!_options.CheckAllHosts)
                    {
                        break;
                    }
                }
            }

            return errorList?.GetHealthState(context) ?? HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private WebRequest CreateFtpWebRequest(string host, bool createFile = false, NetworkCredential? credentials = null)
    {
#pragma warning disable SYSLIB0014 // Type or member is obsolete, see https://github.com/dotnet/docs/issues/27028
        FtpWebRequest ftpRequest;

        if (createFile)
        {
            ftpRequest = (FtpWebRequest)WebRequest.Create($"{host}/beatpulse");

            if (credentials != null)
            {
                ftpRequest.Credentials = credentials;
            }

            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

            using var stream = ftpRequest.GetRequestStream();
            stream.Write(new byte[] { 0x0 }, 0, 1);
        }
        else
        {
            ftpRequest = (FtpWebRequest)WebRequest.Create(host);

            if (credentials != null)
            {
                ftpRequest.Credentials = credentials;
            }

            ftpRequest.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
        }

        return ftpRequest;
#pragma warning restore SYSLIB0014 // Type or member is obsolete
    }
}
