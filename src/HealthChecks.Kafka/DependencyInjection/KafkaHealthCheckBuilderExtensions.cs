using Confluent.Kafka;
using HealthChecks.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="KafkaHealthCheck"/>.
    /// </summary>
    public static class KafkaHealthCheckBuilderExtensions
    {
        private const string NAME = "kafka";
        internal const string DEFAULT_TOPIC = "healthchecks-topic";

        /// <summary>
        /// Add a health check for Kafka cluster.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="config">The Kafka connection configuration parameters to be used.</param>
        /// <param name="topic">The topic name to produce Kafka messages on.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddKafka(
            this IHealthChecksBuilder builder,
            ProducerConfig config,
            string topic = DEFAULT_TOPIC,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            builder.Services.AddSingleton(_ => new KafkaHealthCheck(new KafkaHealthCheckOptions { Configuration = config, Topic = topic }));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<KafkaHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for Kafka cluster.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the Kafka connection configuration parameters to be used.</param>
        /// <param name="topic">The topic name to produce Kafka messages on.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddKafka(
            this IHealthChecksBuilder builder,
            Action<ProducerConfig> setup,
            string topic = DEFAULT_TOPIC,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            builder.Services.AddSingleton(_ =>
            {
                var config = new ProducerConfig();
                setup?.Invoke(config);
                return new KafkaHealthCheck(new KafkaHealthCheckOptions { Configuration = config, Topic = topic });
            });

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<KafkaHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for Kafka cluster.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="options">Options to configure Kafka health check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddKafka(
            this IHealthChecksBuilder builder,
           KafkaHealthCheckOptions options,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            builder.Services.AddSingleton(_ => new KafkaHealthCheck(options));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<KafkaHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }
    }
}
