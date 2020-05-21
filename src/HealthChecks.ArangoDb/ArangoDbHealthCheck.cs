using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using ArangoDBNetStandard;
using ArangoDBNetStandard.AuthApi;
using ArangoDBNetStandard.Transport.Http;

namespace HealthChecks.ArangoDb
{
    public class ArangoDbHealthCheck
        : IHealthCheck
    {
        private readonly ArangoDbOptions _options;
        public ArangoDbHealthCheck(ArangoDbOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var transport = await GetTransport(_options))
                using (var adb = new ArangoDBClient(transport))
                {
                    var databases = await adb.Database.GetCurrentDatabaseInfoAsync();
                    return databases.Error
                        ? new HealthCheckResult(context.Registration.FailureStatus, $"HealthCheck failed with status code: {databases.Code}.")
                        : HealthCheckResult.Healthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, ex.Message, ex);
            }
        }
        private static async Task<HttpApiTransport> GetTransport(ArangoDbOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.JwtToken))
            {
                return HttpApiTransport.UsingJwtAuth(new Uri(options.HostUri), options.Database, options.JwtToken);
            }

            if (options.IsGenerateJwtTokenBasedOnUserNameAndPassword)
            {
                if (string.IsNullOrWhiteSpace(options.UserName))
                {
                    throw new ArgumentNullException(nameof(options.UserName));
                }
                if (string.IsNullOrWhiteSpace(options.Password))
                {
                    throw new ArgumentNullException(nameof(options.Password));
                }

                var transport = HttpApiTransport.UsingNoAuth(new Uri(options.HostUri), options.Database);
                var authClient = new AuthApiClient(transport);
                var jwtTokenResponse = await authClient.GetJwtTokenAsync(options.UserName, options.Password);
                transport.SetJwtToken(jwtTokenResponse.Jwt);
                return transport;
            }

            if (!string.IsNullOrWhiteSpace(options.UserName) && !string.IsNullOrWhiteSpace(options.Password))
            {
                return HttpApiTransport.UsingBasicAuth(new Uri(options.HostUri), options.Database, options.UserName, options.Password);
            }
            
            return HttpApiTransport.UsingNoAuth(new Uri(options.HostUri), options.Database);
        }
    }
}
