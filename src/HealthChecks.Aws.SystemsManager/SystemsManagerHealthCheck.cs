using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.SystemsManager;

public class SystemsManagerHealthCheck : IHealthCheck
{
    private readonly SystemsManagerOptions _systemsManagerOptions;

    public SystemsManagerHealthCheck(SystemsManagerOptions systemsManagerOptions)
    {
        _systemsManagerOptions = Guard.ThrowIfNull(systemsManagerOptions);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateParametersManagerClient();
            foreach (var parameter in _systemsManagerOptions.Parameters)
            {
                await CheckParameterAsync(client, parameter, cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private IAmazonSimpleSystemsManagement CreateParametersManagerClient()
    {
        var credentialsProvided = _systemsManagerOptions.Credentials is not null;
        var regionProvided = _systemsManagerOptions.RegionEndpoint is not null;
        return (credentialsProvided, regionProvided) switch
        {
            (false, false) => new AmazonSimpleSystemsManagementClient(),
            (false, true) => new AmazonSimpleSystemsManagementClient(_systemsManagerOptions.RegionEndpoint),
            (true, false) => new AmazonSimpleSystemsManagementClient(_systemsManagerOptions.Credentials),
            (true, true) => new AmazonSimpleSystemsManagementClient(_systemsManagerOptions.Credentials, _systemsManagerOptions.RegionEndpoint)
        };
    }

    private async Task CheckParameterAsync(IAmazonSimpleSystemsManagement client, string parameterName,
                                                 CancellationToken cancellationToken)
    {
        var request = new GetParameterRequest
        {
            Name = parameterName,
            WithDecryption = true
        };

        _ = await client.GetParameterAsync(request, cancellationToken);
    }
}
