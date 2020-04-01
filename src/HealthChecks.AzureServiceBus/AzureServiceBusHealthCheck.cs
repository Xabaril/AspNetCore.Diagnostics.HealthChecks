using System;
using Microsoft.Azure.ServiceBus;

namespace HealthChecks.AzureServiceBus
{
    public class AzureServiceBusHealthCheck
    {
        public string EntityPath => ConnectionStringBuilder.EntityPath;
        public string Endpoint => ConnectionStringBuilder.Endpoint;

        public bool RequiresSession { get; }
        protected Action<Message> ConfiguringMessage { get; }
        protected ServiceBusConnectionStringBuilder ConnectionStringBuilder { get; }

        protected AzureServiceBusHealthCheck(string connectionString, Action<Message> configuringMessage, bool requiresSession)
            : this(configuringMessage, requiresSession)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            ConnectionStringBuilder = new ServiceBusConnectionStringBuilder(connectionString);
            ConfiguringMessage = configuringMessage;
            RequiresSession = requiresSession;
        }

        protected AzureServiceBusHealthCheck(string connectionString, string entityName, Action<Message> configuringMessage, bool requiresSession)
            : this(configuringMessage, requiresSession)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentNullException(nameof(entityName));
            }

            ConnectionStringBuilder = new ServiceBusConnectionStringBuilder(connectionString)
            {
                EntityPath = entityName
            };

            ConfiguringMessage = configuringMessage;
            RequiresSession = requiresSession;
        }

        private AzureServiceBusHealthCheck(Action<Message> configuringMessage, bool requiresSession)
        {
            ConfiguringMessage = configuringMessage;
            RequiresSession = requiresSession;
        }
    }
}
