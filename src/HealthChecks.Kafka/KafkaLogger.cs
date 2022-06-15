using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace HealthChecks.Kafka
{
    public class KafkaLogger
    {
        private readonly ILogger _logger;

        public KafkaLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Handler(IClient _, LogMessage logMessage)
        {
            var level = (LogLevel)logMessage.LevelAs(LogLevelType.MicrosoftExtensionsLogging);

            _logger.Log(level, "Name={Name},Facility={Facility},Message={Message}",
                logMessage.Name, logMessage.Facility, logMessage.Message);
        }
    }
}
