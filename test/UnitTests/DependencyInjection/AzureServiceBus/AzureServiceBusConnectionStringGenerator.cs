namespace UnitTests.DependencyInjection.AzureServiceBus
{
    public static class AzureServiceBusConnectionStringGenerator
    {
        private static readonly string NamespaceScopedConnectionStringFormat =
            "Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName=DummyAccessKeyName;SharedAccessKey=5dOntTRytoC24opYThisAsit3is2B+OGY1US/fuL3ly=";

        private static readonly string EntityScopedConnectionStringFormat = $"{NamespaceScopedConnectionStringFormat};EntityPath={{1}}";

        /// <summary>
        /// Generates a valid connection string for Azure Service Bus scoped to the namespace
        /// </summary>
        /// <param name="namespaceName">Name of namespace to use</param>
        /// <returns>Valid connection string for Azure Service Bus</returns>
        public static string Generate(string namespaceName)
        {
            return string.Format(NamespaceScopedConnectionStringFormat, namespaceName);
        }

        /// <summary>
        /// Generates a valid connection string for Azure Service Bus scoped to a specific entity
        /// </summary>
        /// <param name="namespaceName">Name of namespace to use</param>
        /// <param name="entityName">Name of the entity in the namespace to scope the access to</param>
        /// <returns>Valid connection string for Azure Service Bus</returns>
        public static string Generate(string namespaceName, string entityName)
        {
            return string.Format(EntityScopedConnectionStringFormat, namespaceName, entityName);
        }
    }
}
