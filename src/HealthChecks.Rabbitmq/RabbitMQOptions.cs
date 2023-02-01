using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ;

/// <summary>
/// Options for <see cref="RabbitMQHealthCheck"/>.
/// </summary>
public class RabbitMQOptions
{
    public Uri Uri { get; set; }
    public SslOption? Ssl { get; set; }

    public RabbitMQOptions(Uri uri)
    {
        Uri = uri;
    }
}
