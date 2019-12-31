using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SharpCifs.Smb;

namespace HealthChecks.SmbCifs
{
    public class SmbCifsStorageHealthCheck : IHealthCheck
    {
        private readonly object _cifsOptions;

        private NtlmPasswordAuthentication _auth;

        public SmbCifsStorageHealthCheck(SmbCifsBasicOptions cifsOptions)
        {
            _cifsOptions = cifsOptions ?? throw new ArgumentNullException(nameof(cifsOptions));

            if (_cifsOptions.GetType() != typeof(SmbCifsBasicOptions))
                throw new InvalidCastException($"Invalid cast for object {nameof(cifsOptions)} to SmbCifsBasicOptions");
        }

        public SmbCifsStorageHealthCheck(SmbCifsExtendedOptions cifsOptions)
        {
            _cifsOptions = cifsOptions ?? throw new ArgumentNullException(nameof(cifsOptions));

            if (_cifsOptions.GetType() != typeof(SmbCifsExtendedOptions))
                throw new InvalidCastException($"Invalid cast for object {nameof(cifsOptions)} to SmbCifsExtendedOptions");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
          HealthCheckContext context,
          CancellationToken cancellationToken = default)
        {
            var hostName = _cifsOptions.GetType() == typeof(SmbCifsBasicOptions)
                ? ((SmbCifsBasicOptions)_cifsOptions).Hostname
                : ((SmbCifsExtendedOptions)_cifsOptions).Hostname;

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

            if (_cifsOptions.GetType() == typeof(SmbCifsBasicOptions))
            {
                var options = (SmbCifsBasicOptions)_cifsOptions;
                if (options.Hostname == null)
                    throw new ArgumentNullException(nameof(options.Hostname));
                if (options.Domain == null)
                    throw new ArgumentNullException(nameof(options.Domain));
                if (options.Username == null)
                    throw new ArgumentNullException(nameof(options.Username));
                if (options.UserPassword == null)
                    throw new ArgumentNullException(nameof(options.UserPassword));


                _auth = new NtlmPasswordAuthentication(options.Domain, options.Username, options.UserPassword);
            }
            else if (_cifsOptions.GetType() == typeof(SmbCifsExtendedOptions))
            {
                var options = (SmbCifsExtendedOptions)_cifsOptions;
                if (options.Hostname == null)
                    throw new ArgumentNullException(nameof(options.Hostname));
                if (options.Domain == null)
                    throw new ArgumentNullException(nameof(options.Domain));
                if (options.Username == null)
                    throw new ArgumentNullException(nameof(options.Username));
                //Not Sure this are really all mandatory on this case
                if (options.Challenge == null)
                    throw new ArgumentNullException(nameof(options.Challenge));
                if (options.AnsiHash == null)
                    throw new ArgumentNullException(nameof(options.AnsiHash));
                if (options.UnicodeHash == null)
                    throw new ArgumentNullException(nameof(options.UnicodeHash));

                _auth = new NtlmPasswordAuthentication(options.Domain, options.Username, options.Challenge, options.AnsiHash, options.UnicodeHash);
            }





        }
    }
}

