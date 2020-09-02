using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Network.Extensions;

namespace HealthChecks.Network
{
    public class FtpHealthCheck
        : IHealthCheck
    {
        private readonly FtpHealthCheckOptions _options;
        public FtpHealthCheck(FtpHealthCheckOptions options)
        {
            _options = options ?? throw new ArgumentException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var (host, createFile, credentials) in _options.Hosts.Values)
                {
                    var ftpRequest = CreateFtpWebRequest(host, createFile, credentials);

                    using (var ftpResponse = (FtpWebResponse)await ftpRequest.GetResponseAsync().WithCancellationTokenAsync(cancellationToken))
                    {
                        if (ftpResponse.StatusCode != FtpStatusCode.PathnameCreated
                            && ftpResponse.StatusCode != FtpStatusCode.ClosingData)
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Error connecting to ftp host {host} with exit code {ftpResponse.StatusCode}");
                        }
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
        private WebRequest CreateFtpWebRequest(string host, bool createFile = false, NetworkCredential credentials = null)
        {
            FtpWebRequest ftpRequest;

            if (createFile)
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create($"{host}/beatpulse");

                if (credentials != null)
                {
                    ftpRequest.Credentials = credentials;
                }

                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                using (var stream = ftpRequest.GetRequestStream())
                {
                    stream.Write(new byte[] { 0x0 }, 0, 1);
                }
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
        }
    }
}
