using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.SystemsManager;

public class SystemsManagerHealthCheck : IHealthCheck
{
    private readonly SystemsManagerOptions _systemsManagerOptions;

    public SystemsManagerHealthCheck(SystemsManagerOptions systemsManagerOptions)
    {
        _systemsManagerOptions = systemsManagerOptions ?? throw new ArgumentNullException(nameof(systemsManagerOptions));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateParametersManagerClient();
            foreach (var parameter in _systemsManagerOptions.Parameters)
            {
                _ = await GetParameterAsync(client, parameter, cancellationToken);
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
        IAmazonSimpleSystemsManagement client;

        var credentialsProvided = _systemsManagerOptions.Credentials is not null;

        var regionProvided = _systemsManagerOptions.RegionEndpoint is not null;

        if (!credentialsProvided && !regionProvided)
        {
            client = new AmazonSimpleSystemsManagementClient();
        }
        else if (!credentialsProvided && regionProvided)
        {
            client = new AmazonSimpleSystemsManagementClient(_systemsManagerOptions.RegionEndpoint);
        }
        else if (credentialsProvided && regionProvided)
        {
            client = new AmazonSimpleSystemsManagementClient(_systemsManagerOptions.Credentials,
                _systemsManagerOptions.RegionEndpoint);
        }
        else
        {
            client = new AmazonSimpleSystemsManagementClient(_systemsManagerOptions.Credentials);
        }

        return client;
    }

    private async Task<string> GetParameterAsync(IAmazonSimpleSystemsManagement client, string parameterName,
                                                 CancellationToken cancellationToken)
    {
        var request = new GetParameterRequest()
        {
            Name = parameterName,
            WithDecryption = true
        };

        var response = await client.GetParameterAsync(request, cancellationToken);

        return response.Parameter.Value;
    }
}
