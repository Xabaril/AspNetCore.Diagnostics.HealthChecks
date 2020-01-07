using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SharpCifs.Smb;

namespace HealthChecks.SmbCifs
{
    public class SmbCifsStorageHealthCheck : IHealthCheck
    {
        private readonly SmbCifsOptions _cifsOptions;

        private NtlmPasswordAuthentication _auth;

        public SmbCifsStorageHealthCheck(SmbCifsOptions cifsOptions)
        {
            _cifsOptions = cifsOptions ?? throw new ArgumentNullException(nameof(cifsOptions));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
          HealthCheckContext context,
          CancellationToken cancellationToken = default)
        {
            var hostName = _cifsOptions.Hostname;

            try
            {
                CreateConnection();

                //try connect and list
                new SmbFile($"smb://{hostName}/", _auth).List();

                return await Task.FromResult(HealthCheckResult.Healthy());

            }
            catch (Exception ex)
            {
                return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, null, ex));
            }
        }

        private void CreateConnection()
        {
            if (_cifsOptions is SmbCifsBasicOptions optionsBasic)
            {
                if (optionsBasic.Hostname == null)
                    throw new ArgumentNullException(nameof(optionsBasic.Hostname));
                if (optionsBasic.Domain == null)
                    throw new ArgumentNullException(nameof(optionsBasic.Domain));
                if (optionsBasic.Username == null)
                    throw new ArgumentNullException(nameof(optionsBasic.Username));
                if (optionsBasic.UserPassword == null)
                    throw new ArgumentNullException(nameof(optionsBasic.UserPassword));

                _auth = new NtlmPasswordAuthentication(optionsBasic.Domain, optionsBasic.Username, optionsBasic.UserPassword);
            }
            else if (_cifsOptions is SmbCifsExtendedOptions optionsExtended)
            {
                if (optionsExtended.Hostname == null)
                    throw new ArgumentNullException(nameof(optionsExtended.Hostname));
                if (optionsExtended.Domain == null)
                    throw new ArgumentNullException(nameof(optionsExtended.Domain));
                if (optionsExtended.Username == null)
                    throw new ArgumentNullException(nameof(optionsExtended.Username));
                if (optionsExtended.Challenge == null)
                    throw new ArgumentNullException(nameof(optionsExtended.Challenge));
                if (optionsExtended.AnsiHash == null)
                    throw new ArgumentNullException(nameof(optionsExtended.AnsiHash));
                if (optionsExtended.UnicodeHash == null)
                    throw new ArgumentNullException(nameof(optionsExtended.UnicodeHash));

                _auth = new NtlmPasswordAuthentication(optionsExtended.Domain, optionsExtended.Username, optionsExtended.Challenge, optionsExtended.AnsiHash, optionsExtended.UnicodeHash);
            }





        }
    }
}
