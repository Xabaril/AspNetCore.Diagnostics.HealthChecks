using HealthChecks.UI.K8s.Operator.Diagnostics;
using HealthChecks.UI.K8s.Operator.Handlers;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.K8s.Operator.Operator;

internal sealed class ClusterServiceWatcher : IDisposable
{
    private readonly IKubernetes _client;
    private readonly ILogger<K8sOperator> _logger;
    private readonly OperatorDiagnostics _diagnostics;
    private readonly NotificationHandler _notificationHandler;
    private Watcher<V1Service>? _watcher;

    public ClusterServiceWatcher(
      IKubernetes client,
      ILogger<K8sOperator> logger,
      OperatorDiagnostics diagnostics,
      NotificationHandler notificationHandler
      )
    {
        _client = Guard.ThrowIfNull(client);
        _logger = Guard.ThrowIfNull(logger);
        _diagnostics = Guard.ThrowIfNull(diagnostics);
        _notificationHandler = Guard.ThrowIfNull(notificationHandler);
    }

    internal Task Watch(HealthCheckResource resource, CancellationToken token)
    {
        var response = _client.CoreV1.ListServiceForAllNamespacesWithHttpMessagesAsync(
            labelSelector: $"{resource.Spec.ServicesLabel}",
            watch: true,
            cancellationToken: token);

        // KubernetesClient 21'de Watch geri çağırım API'si "ileride kaldırılacak" diye işaretli ama çalışıyor;
        // WatchAsync (IAsyncEnumerable) geçişi davranış değişikliği olduğu için ayrı bir işe bırakıldı.
#pragma warning disable CS0618
        _watcher = response.Watch<V1Service, V1ServiceList>(
            onEvent: async (type, item) => await _notificationHandler.NotifyDiscoveredServiceAsync(type, item, resource),
            onError: e =>
            {
                _diagnostics.ServiceWatcherThrow(e);
                Watch(resource, token);
            }
        );
#pragma warning restore CS0618

        _diagnostics.ServiceWatcherStarting("All");

        return Task.CompletedTask;
    }

    internal void Stopwatch(/*HealthCheckResource resource*/)
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_watcher != null && _watcher.Watching)
        {
            _watcher.Dispose();
        }
    }
}
