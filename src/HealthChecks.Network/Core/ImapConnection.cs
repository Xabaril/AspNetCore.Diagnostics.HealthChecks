using HealthChecks.Network.Extensions;

namespace HealthChecks.Network.Core;

internal class ImapConnection : MailConnection
{
    public bool IsAuthenticated { get; private set; }

    public bool Connected => _tcpClient?.Connected == true;

    public ImapConnectionType ConnectionType
    {
        get => _connectionType;

        private set
        {
            _connectionType = value;
            UseSSL = ConnectionType == ImapConnectionType.SSL_TLS ? true : false;
        }
    }

    private ImapConnectionType _connectionType;

    internal ImapConnection(ImapConnectionOptions options)
        : base(options.Host, options.Port, true, options.AllowInvalidRemoteCertificates)
    {
        Guard.ThrowIfNull(options);

        ConnectionType = options.ConnectionType;
        ComputeDefaultValues();
    }

    private void ComputeDefaultValues()
    {
        switch (ConnectionType)
        {
            case ImapConnectionType.AUTO when Port == 993:
                ConnectionType = ImapConnectionType.SSL_TLS;
                break;
            case ImapConnectionType.AUTO when Port == 143:
                ConnectionType = ImapConnectionType.STARTTLS;
                break;
        }

        if (ConnectionType == ImapConnectionType.AUTO)
        {
            throw new Exception($"Port {Port} is not a valid imap port when using automatic configuration");
        }
    }

    public async Task<bool> AuthenticateAsync(string user, string password, CancellationToken cancellationToken = default)
    {
        if (ConnectionType == ImapConnectionType.STARTTLS)
        {
            await UpgradeToSecureConnectionAsync(cancellationToken).ConfigureAwait(false);
        }

        IsAuthenticated = await ExecuteCommandAsync(
            ImapCommands.Login(user, password),
            result =>
            {
                var AUTHFAILED = "AUTHENTICATIONFAILED"u8;
                return !result.ContainsArray(AUTHFAILED);
            },
            cancellationToken).ConfigureAwait(false);
        return IsAuthenticated;
    }

    private async Task<bool> UpgradeToSecureConnectionAsync(CancellationToken cancellationToken)
    {
        var upgradeSuccess = await ExecuteCommandAsync(
            ImapCommands.StartTLS(),
            result =>
            {
                var OK_TLS_NEGOTIATION = "OK Begin TLS"u8;
                return result.ContainsArray(OK_TLS_NEGOTIATION);
            },
            cancellationToken).ConfigureAwait(false);
        if (upgradeSuccess)
        {
            ConnectionType = ImapConnectionType.SSL_TLS;
            _stream = await GetStreamAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        else
        {
            throw new Exception("Could not upgrade IMAP non SSL connection using STARTTLS handshake");
        }
    }

    public async Task<bool> SelectFolderAsync(string folder, CancellationToken cancellationToken = default)
    {
        return await ExecuteCommandAsync(
            ImapCommands.SelectFolder(folder),
            result =>
            {
                var OK = "& OK"u8;
                var ERROR = "& NO"u8;
                //Double check, some servers sometimes include a last line with a & OK appending extra info when command fails
                return result.ContainsArray(OK) && !result.ContainsArray(ERROR);
            },
            cancellationToken).ConfigureAwait(false);
    }
}
