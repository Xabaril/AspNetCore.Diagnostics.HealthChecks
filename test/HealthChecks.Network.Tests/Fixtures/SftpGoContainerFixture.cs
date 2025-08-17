using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HealthChecks.Network.Tests.Fixtures;

public class SftpGoContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "drakkan/sftpgo";

    private const string Tag = "v2.6.6-distroless-slim";

    private const int FtpPort = 21;

    private const int SftpPort = 2022;

    private const int ApiPort = 8080;

    private const string Username = "user";

    private const string Password = "password";

    private const string Passphrase = "beatpulse";

    private string? _privateKey;

    public IContainer? Container { get; private set; }

    public (string Hostname, int Port, string Username, string Password) GetFtpConnectionProperties()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return (Container.Hostname, Container.GetMappedPublicPort(FtpPort), Username, Password);
    }

    public (string Hostname, int Port, string Username, string Password, string PrivateKey, string Passphrase) GetSftpConnectionProperties()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        if (_privateKey is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return (Container.Hostname, Container.GetMappedPublicPort(SftpPort), Username, Password, _privateKey, Passphrase);
    }

    public async Task InitializeAsync()
    {
        Container = await CreateContainerAsync();

        // Initialize SFTPGo via its HTTP API: get admin token and create a user
        await InitializeSftpGoAsync(Container);
    }

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private async Task<IContainer> CreateContainerAsync()
    {
        var waitStrategy = Wait
            .ForUnixContainer()
            .UntilMessageIsLogged(".+server listener registered.+");

        var passivePorts = FindPortRangeForPassiveMode(count: 3);

        int rangeStart = passivePorts[0];
        int rangeEnd = passivePorts[^1];

        var builder = new ContainerBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .WithEnvironment("STPFGO_DATA_PROVIDER__DRIVER", "memory")
            .WithEnvironment("SFTPGO_DATA_PROVIDER__UPDATE_MODE", "0")
            .WithEnvironment("SFTPGO_DATA_PROVIDER__CREATE_DEFAULT_ADMIN", "true")
            .WithEnvironment("SFTPGO_DEFAULT_ADMIN_USERNAME", Username)
            .WithEnvironment("SFTPGO_DEFAULT_ADMIN_PASSWORD", Password)
            .WithEnvironment("SFTPGO_FTPD__BINDINGS__0__PORT", FtpPort.ToString())
            .WithEnvironment("SFTPGO_FTPD__PASSIVE_PORT_RANGE__START", rangeStart.ToString())
            .WithEnvironment("SFTPGO_FTPD__PASSIVE_PORT_RANGE__END", rangeEnd.ToString())
            .WithPortBinding(FtpPort, true)
            .WithPortBinding(SftpPort, true)
            .WithPortBinding(ApiPort, true)
            .WithWaitStrategy(waitStrategy);

        foreach (int port in passivePorts)
        {
            builder = builder.WithPortBinding(port, port);
        }

        var container = builder.Build();

        await container.StartAsync();

        return container;
    }

    private async Task InitializeSftpGoAsync(IContainer container)
    {
        string keysPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "sftpgo", "keys");

        string privateKeyPath = Path.Combine(keysPath, "id_rsa");
        string publicKeyPath = Path.Combine(keysPath, "id_rsa.pub");

        _privateKey = await File.ReadAllTextAsync(privateKeyPath);
        string publicKey = await File.ReadAllTextAsync(publicKeyPath);

        var uriBuilder = new UriBuilder(
            Uri.UriSchemeHttp,
            container.Hostname,
            container.GetMappedPublicPort(ApiPort),
            "/api/v2/");

        using var client = new HttpClient();

        client.BaseAddress = uriBuilder.Uri;

        string token = await GetAdminTokenAsync(client);

        await CreateOrUpsertUserAsync(client, token, publicKey);
    }

    private static async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "token");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            nameof(AuthenticationSchemes.Basic),
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}")));

        using var response = await client.SendAsync(request);

        await using var stream = await response.Content.ReadAsStreamAsync();

        var json = await JsonDocument.ParseAsync(stream);

        return json.RootElement.GetProperty("access_token").GetString()!;
    }

    private static async Task CreateOrUpsertUserAsync(HttpClient client, string token, string publicKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "users");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            token);

        var payload = new
        {
            id = 1,
            status = 1,
            username = Username,
            email = $"{Username}@healthchecks.com",
            password = Password,
            public_keys = new[] { publicKey },
            permissions = new Dictionary<string, string[]>
            {
                ["/"] = ["*"]
            }
        };

        string json = JsonSerializer.Serialize(payload);

        request.Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        await client.SendAsync(request);
    }

    private static IReadOnlyList<int> FindPortRangeForPassiveMode(int count, int rangeStart = 50000, int rangeEnd = 65000)
    {
        int totalSpan = rangeEnd - rangeStart + 1 - (count - 1);

        int randomOffset = Random.Shared.Next(0, totalSpan);

        for (int i = 0; i < totalSpan; i++)
        {
            int start = rangeStart + (randomOffset + i) % totalSpan;

            var listeners = new List<TcpListener>(capacity: count);

            try
            {
                for (int p = start; p < start + count; p++)
                {
                    var l = new TcpListener(IPAddress.Loopback, p);

                    l.Server.ExclusiveAddressUse = true;

                    l.Start();

                    listeners.Add(l);
                }

                int[] result = Enumerable.Range(start, count).ToArray();

                foreach (var l in listeners)
                {
                    l.Stop();
                }

                return result;
            }
            catch
            {
                foreach (var l in listeners)
                {
                    try
                    {
                        l.Stop();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        throw new InvalidOperationException($"Unable to find {count} consecutive free ports in range {rangeStart}-{rangeEnd}.");
    }
}
