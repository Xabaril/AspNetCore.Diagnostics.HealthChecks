using System.Collections.ObjectModel;
using ArangoDBNetStandard;
using ArangoDBNetStandard.AuthApi;
using ArangoDBNetStandard.Transport.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.ArangoDb;

public class ArangoDbHealthCheck : IHealthCheck
{
    private readonly ArangoDbOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.type", nameof(ArangoDbHealthCheck) },
                    { "db.system.name", "arango" }
    };

    public ArangoDbHealthCheck(ArangoDbOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            using var transport = await GetTransportAsync(_options).ConfigureAwait(false);

            using var adb = new ArangoDBClient(transport);
            var databases = await adb.Database.GetCurrentDatabaseInfoAsync(cancellationToken).ConfigureAwait(false);
            return databases.Error
                ? new HealthCheckResult(context.Registration.FailureStatus, $"HealthCheck failed with status code: {databases.Code}.", data: new ReadOnlyDictionary<string, object>(checkDetails))
                : HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }

    private static async Task<HttpApiTransport> GetTransportAsync(ArangoDbOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.JwtToken))
        {
            return HttpApiTransport.UsingJwtAuth(new Uri(options.HostUri), options.Database, options.JwtToken);
        }

        if (options.IsGenerateJwtTokenBasedOnUserNameAndPassword)
        {
            if (string.IsNullOrWhiteSpace(options.UserName))
            {
                throw new ArgumentNullException(nameof(options), $"{nameof(options.UserName)} must be set when {nameof(options.IsGenerateJwtTokenBasedOnUserNameAndPassword)} is enabled");
            }
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                throw new ArgumentNullException(nameof(options), $"{nameof(options.Password)} must be set when {nameof(options.IsGenerateJwtTokenBasedOnUserNameAndPassword)} is enabled");
            }

            var transport = HttpApiTransport.UsingNoAuth(new Uri(options.HostUri), options.Database);
            var authClient = new AuthApiClient(transport);
            var jwtTokenResponse = await authClient.GetJwtTokenAsync(options.UserName, options.Password).ConfigureAwait(false);
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
