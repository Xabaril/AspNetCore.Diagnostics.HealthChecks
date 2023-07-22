using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ;

/// <summary>
/// Options for <see cref="RabbitMQHealthCheck"/>.
/// </summary>
public class RabbitMQHealthCheckOptions
{
    /// <summary>
    /// An already created connection to RabbitMQ.
    /// It has priority over <see cref="ConnectionFactory"/> and <see cref="ConnectionUri"/>.
    /// </summary>
    public IConnection? Connection { get; set; }

    /// <summary>
    /// A connection factory for RabbitMQ.
    /// It has priority over <see cref="ConnectionUri"/>.
    /// </summary>
    public IConnectionFactory? ConnectionFactory { get; set; }

    /// <summary>
    /// An Uri representing a connection string for RabbitMQ.
    /// </summary>
    /// <remarks>
    /// Can be used in conjunction with the <see cref="Ssl"/> property.
    /// </remarks>
    public Uri? ConnectionUri { get; set; }

    /// <summary>
    /// The SSL options for a connection string.
    /// </summary>
    /// <remarks>
    /// Must be used in conjunction with the <see cref="ConnectionUri"/> property.
    /// </remarks>
    public SslOption? Ssl { get; set; }

    /// <summary>
    /// Timeout setting for connection attempts.
    /// </summary>
    public TimeSpan? RequestedConnectionTimeout { get; set; }
}
