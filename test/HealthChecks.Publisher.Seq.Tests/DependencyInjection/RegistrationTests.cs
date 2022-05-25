namespace HealthChecks.Publisher.Datadog.Tests.DependencyInjection
{
    public class seq_publisher_registration_should
    {
        [Fact]
        public void add_healthcheck_when_properly_configured()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddSeqPublisher(setup =>
                    new SeqOptions()
                    {
                        ApiKey = "apiKey",
                        DefaultInputLevel = Seq.SeqInputLevel.Information,
                        Endpoint = "endpoint"
                    });

            using var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }
    }
}
