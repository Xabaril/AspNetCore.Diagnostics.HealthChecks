namespace HealthChecks.Nats.Tests;

internal class Defines
{
    public const string DefaultLocalConnectionString = "nats://localhost:4222";
    public const string DemoConnectionString = "nats://demo.nats.io:4222";
    public const string MixedLocalUrl = "nats://localhost:4222, nats://localhost:8222";
    public const string ConnectionStringDoesNotExistOrStopped = "nats://DoesNotExist:4222";

    public const string CredentialsPathDoesnExist = nameof(CredentialsPathDoesnExist);

    public const string NatsName = "nats";
    public const string CustomRegistrationName = nameof(CustomRegistrationName);
    public static readonly string[] Tags = new string[] { NatsName };

    public const string HealthRequestRelativePath = "/health";
}
