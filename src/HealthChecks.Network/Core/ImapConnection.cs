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

        var result = await ExecuteCommandAsync(ImapCommands.Login(user, password), cancellationToken).ConfigureAwait(false);
        IsAuthenticated = !result.Contains(ImapResponse.AUTHFAILED);
        return IsAuthenticated;
    }

    private async Task<bool> UpgradeToSecureConnectionAsync(CancellationToken cancellationToken)
    {
        var commandResult = await ExecuteCommandAsync(ImapCommands.StartTLS(), cancellationToken).ConfigureAwait(false);
        var upgradeSuccess = commandResult.Contains(ImapResponse.OK_TLS_NEGOTIATION);
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

    public async Task<bool> SelectFolderAsync(string folder, CancellationToken cancellationToken = default) //TODO: public API change
    {
        var result = await ExecuteCommandAsync(ImapCommands.SelectFolder(folder), cancellationToken).ConfigureAwait(false);

        //Double check, some servers sometimes include a last line with a & OK appending extra info when command fails
        return result.Contains(ImapResponse.OK) && !result.Contains(ImapResponse.ERROR);
    }

    public async Task<string> GetFoldersAsync(CancellationToken cancellationToken = default) //TODO: public API change
    {
        return await ExecuteCommandAsync(ImapCommands.ListFolders(), cancellationToken).ConfigureAwait(false);
    }
}
