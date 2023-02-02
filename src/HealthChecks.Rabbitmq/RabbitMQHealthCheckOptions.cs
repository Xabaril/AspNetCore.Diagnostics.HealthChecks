using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ;

/// <summary>
/// Options for <see cref="RabbitMQHealthCheck"/>.
/// </summary>
public class RabbitMQHealthCheckOptions
{
    /// <summary>
    /// An already created connection to RabbitMQ.
    /// </summary>
    public IConnection? Connection { get; set; }

    /// <summary>
    /// A connection factory for RabbitMQ.
    /// </summary>
    public IConnectionFactory? ConnectionFactory { get; set; }

    /// <summary>
    /// An Uri representing a  connection string for RabbitMQ.
    /// </summary>
    /// <remarks>
    /// Can be used in conjunction with the <see cref="SslOption"/> property.
    /// </remarks>
    public Uri? ConnectionUri { get; set; }

    /// <summary>
    /// The SSL options for a connection string.
    /// </summary>
    /// <remarks>
    /// Must be used in conjunction with the <see cref="ConnectionUri"/> property.
    /// </remarks>
    public SslOption? Ssl { get; set; }
}
