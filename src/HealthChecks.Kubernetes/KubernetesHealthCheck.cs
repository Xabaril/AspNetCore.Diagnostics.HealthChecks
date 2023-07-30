using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kubernetes;

public class KubernetesHealthCheck : IHealthCheck
{
    private readonly KubernetesHealthCheckBuilder _builder;
    private readonly k8s.Kubernetes _client;

    public KubernetesHealthCheck(KubernetesHealthCheckBuilder builder, k8s.Kubernetes client)
    {
        _builder = Guard.ThrowIfNull(builder);
        _client = Guard.ThrowIfNull(client);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        List<Task<(bool result, string name)>> checks = new();

        try
        {
            foreach (var item in _builder.Options.Registrations)
            {
                checks.Add(KubernetesChecksExecutor.CheckAsync(_client, item, cancellationToken));
            }

            var results = await Task.WhenAll(checks).PreserveMultipleExceptions().ConfigureAwait(false);

            if (results.Any(r => !r.result))
            {
                var resultsNotMeetingConditions = results.Where(r => !r.result).Select(r => r.name);
                return new HealthCheckResult(context.Registration.FailureStatus,
                    $"Kubernetes resources with failed conditions: {string.Join(",", resultsNotMeetingConditions)}");
            }

            return HealthCheckResult.Healthy();
        }
        catch (AggregateException ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, string.Join(",", ex.InnerExceptions.Select(s => s.Message)));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, ex.Message, ex);
        }
    }
}
