using System.Text;
using HealthChecks.Network.Extensions;

namespace HealthChecks.Network.Core;

internal class SmtpConnection : MailConnection
{
    private readonly SmtpConnectionOptions _options;
    private SmtpConnectionType _connectionType;

    public SmtpConnectionType ConnectionType
    {
        get => _connectionType;

        private set
        {
            _connectionType = value;
            UseSSL = ConnectionType == SmtpConnectionType.SSL ? true : false;
        }
    }

    public SmtpConnection(SmtpConnectionOptions options)
        : base(options.Host, options.Port, false, options.AllowInvalidRemoteCertificates)
    {
        _options = Guard.ThrowIfNull(options);
        ConnectionType = _options.ConnectionType;
        ComputeDefaultValues();
    }

    public new async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        await base.ConnectAsync(cancellationToken).ConfigureAwait(false);
        return await ExecuteCommandAsync(
            SmtpCommands.EHLO(Host),
            result =>
            {
                var ACTION_OK = "250"u8;
                return result.ContainsArray(ACTION_OK);
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        if (ShouldUpgradeConnection)
        {
            await UpgradeToSecureConnectionAsync(cancellationToken).ConfigureAwait(false);
        }
        await ExecuteCommandAsync(SmtpCommands.EHLO(Host), _ => true, cancellationToken).ConfigureAwait(false);
        await ExecuteCommandAsync(SmtpCommands.AUTHLOGIN(), _ => true, cancellationToken).ConfigureAwait(false);
        await ExecuteCommandAsync($"{ToBase64(userName)}\r\n", _ => true, cancellationToken).ConfigureAwait(false);

        password = password?.Length > 0 ? ToBase64(password) : "";

        return await ExecuteCommandAsync(
            $"{password}\r\n",
            result =>
            {
                var AUTHENTICATION_SUCCESS = "235"u8;
                return result.ContainsArray(AUTHENTICATION_SUCCESS);
            },
            cancellationToken).ConfigureAwait(false);
    }

    private bool ShouldUpgradeConnection => !UseSSL && _connectionType != SmtpConnectionType.PLAIN;

    private void ComputeDefaultValues()
    {
        switch (_options.ConnectionType)
        {
            case SmtpConnectionType.AUTO when Port == 465:
                ConnectionType = SmtpConnectionType.SSL;
                break;
            case SmtpConnectionType.AUTO when Port == 587:
                ConnectionType = SmtpConnectionType.TLS;
                break;
            case SmtpConnectionType.AUTO when Port == 25:
                ConnectionType = SmtpConnectionType.PLAIN;
                break;
        }

        if (ConnectionType == SmtpConnectionType.AUTO)
        {
            throw new Exception($"Port {Port} is not a valid smtp port when using automatic configuration");
        }
    }

    private async Task<bool> UpgradeToSecureConnectionAsync(CancellationToken cancellationToken)
    {
        var upgradeResult = await ExecuteCommandAsync(
            SmtpCommands.STARTTLS(),
            result =>
            {
                var SERVICE_READY = "220"u8;
                return result.ContainsArray(SERVICE_READY);
            },
            cancellationToken).ConfigureAwait(false);
        if (upgradeResult)
        {
            UseSSL = true;
            _stream = await GetStreamAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        else
        {
            throw new Exception("Could not upgrade SMTP non SSL connection using STARTTLS handshake");
        }
    }

    private string ToBase64(string text)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    }
}
