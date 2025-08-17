using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.Network.Tests.Fixtures;

public class DockerMailServerContainerFixture : IAsyncLifetime
{
    protected const string Registry = "docker.io";

    protected const string Image = "mailserver/docker-mailserver";

    protected const string Tag = "15.1.0";

    protected const int ExplicitTlsSmtpPort = 587;

    protected const int ImplicitTlsSmtpPort = 465;

    protected const int ExplicitTlsImapPort = 143;

    protected const int ImplicitTlsImapPort = 993;

    protected const string Email = "user@healthchecks.com";

    protected const string Password = "password";

    public IContainer? Container { get; private set; }

    public (string Host, int ExplicitTlsPort, int ImplicitTlsPort, string Username, string Password) GetSmtpConnectionProperties()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return (
            Container.Hostname,
            Container.GetMappedPublicPort(ExplicitTlsSmtpPort),
            Container.GetMappedPublicPort(ImplicitTlsSmtpPort),
            Email,
            Password
        );
    }

    public (string Host, int ExplicitTlsPort, int ImplicitTlsPort, string Username, string Password) GetImapConnectionProperties()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return (
            Container.Hostname,
            Container.GetMappedPublicPort(ExplicitTlsImapPort),
            Container.GetMappedPublicPort(ImplicitTlsImapPort),
            Email,
            Password
        );
    }

    public async Task InitializeAsync() => Container = await CreateContainerAsync();

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    protected virtual ContainerBuilder Configure(ContainerBuilder builder) => builder;

    private async Task<IContainer> CreateContainerAsync()
    {
        var waitStrategy = Wait
            .ForUnixContainer()
            .UntilCommandIsCompleted("setup", "email", "add", Email, Password)
            .UntilMessageIsLogged(".+ is up and running")
            .UntilMessageIsLogged(".+daemon started.+")
            .UntilPortIsAvailable(ImplicitTlsSmtpPort)
            .UntilPortIsAvailable(ExplicitTlsSmtpPort);

        var builder = new ContainerBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithEnvironment("OVERRIDE_HOSTNAME", "mail.beatpulse.com")
            .WithEnvironment("POSTFIX_INET_PROTOCOLS", "ipv4")
            .WithEnvironment("DOVECOT_INET_PROTOCOLS", "ipv4")
            .WithEnvironment("ENABLE_CLAMAV", "0")
            .WithEnvironment("ENABLE_AMAVIS", "0")
            .WithEnvironment("ENABLE_RSPAMD", "0")
            .WithEnvironment("ENABLE_OPENDKIM", "0")
            .WithEnvironment("ENABLE_OPENDMARC", "0")
            .WithEnvironment("ENABLE_POLICYD_SPF", "0")
            .WithEnvironment("ENABLE_SPAMASSASSIN", "0")
            .WithPortBinding(ExplicitTlsSmtpPort, true)
            .WithPortBinding(ImplicitTlsSmtpPort, true)
            .WithPortBinding(ExplicitTlsImapPort, true)
            .WithPortBinding(ImplicitTlsImapPort, true)
            .WithWaitStrategy(waitStrategy);

        builder = Configure(builder);

        var container = builder.Build();

        await container.StartAsync();

        return container;
    }
}

public class SecureDockerMailServerContainerFixture : DockerMailServerContainerFixture
{
    protected override ContainerBuilder Configure(ContainerBuilder builder)
    {
        var certsPath = new DirectoryInfo(Path.Combine(
            Directory.GetCurrentDirectory(),
            "Resources",
            "docker-mailserver",
            "certs"));

        return builder
            .WithEnvironment("SSL_TYPE", "manual")
            .WithResourceMapping(certsPath, "/tmp/docker-mailserver/certs")
            .WithEnvironment("SSL_CERT_PATH", "/tmp/docker-mailserver/certs/public.crt")
            .WithEnvironment("SSL_KEY_PATH", "/tmp/docker-mailserver/certs/private.key");
    }
}
