using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ;

/// <summary>
/// Options for <see cref="RabbitMQHealthCheck"/>.
/// </summary>
public class RabbitMQHealthCheckOptions
{
    public Uri Uri { get; set; }
    public SslOption? Ssl { get; set; }

    public RabbitMQHealthCheckOptions(Uri uri)
    {
        Uri = uri;
    }
}
