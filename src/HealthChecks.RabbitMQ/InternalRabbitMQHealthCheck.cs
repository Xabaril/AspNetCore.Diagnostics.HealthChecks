using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ
{
    internal class InternalRabbitMQHealthCheck : RabbitMQHealthCheck
    {
        public InternalRabbitMQHealthCheck(string rabbitMqConnectionString, SslOption sslOption = null) : base(rabbitMqConnectionString, sslOption)
        {
            AttemptConnectionReuse = true;
        }

        public InternalRabbitMQHealthCheck(IConnection connection) : base(connection)
        {
            AttemptConnectionReuse = false;
        }

        public InternalRabbitMQHealthCheck(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
            AttemptConnectionReuse = connectionFactory.UseBackgroundThreadsForIO;
        }
    }
}