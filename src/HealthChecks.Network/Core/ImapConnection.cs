namespace HealthChecks.Network.Core
{
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
            if (options == null)
                throw new ArgumentNullException(nameof(options));

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

        public async Task<bool> AuthenticateAsync(string user, string password)
        {
            if (ConnectionType == ImapConnectionType.STARTTLS)
            {
                await UpgradeToSecureConnectionAsync();
            }

            var result = await ExecuteCommand(ImapCommands.Login(user, password));
            IsAuthenticated = !result.Contains(ImapResponse.AUTHFAILED);
            return IsAuthenticated;
        }

        private async Task<bool> UpgradeToSecureConnectionAsync()
        {
            var commandResult = await ExecuteCommand(ImapCommands.StartTLS());
            var upgradeSuccess = commandResult.Contains(ImapResponse.OK_TLS_NEGOTIATION);
            if (upgradeSuccess)
            {
                ConnectionType = ImapConnectionType.SSL_TLS;
                _stream = GetStream();
                return true;
            }
            else
            {
                throw new Exception("Could not upgrade IMAP non SSL connection using STARTTLS handshake");
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        public async Task<bool> SelectFolder(string folder) //TODO: public API change
#pragma warning restore IDE1006 // Naming Styles
        {
            var result = await ExecuteCommand(ImapCommands.SelectFolder(folder));

            //Double check, some servers sometimes include a last line with a & OK appending extra info when command fails
            return result.Contains(ImapResponse.OK) && !result.Contains(ImapResponse.ERROR);
        }

#pragma warning disable IDE1006 // Naming Styles
        public async Task<string> GetFolders() //TODO: public API change
#pragma warning restore IDE1006 // Naming Styles
        {
            return await ExecuteCommand(ImapCommands.ListFolders());
        }
    }
}
