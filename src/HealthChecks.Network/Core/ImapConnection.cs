using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network.Core
{
    internal class ImapConnection : MailConnection
    {
        public bool IsAuthenticated { get; private set; }
        public bool Connected => _tcpClient.Connected;

        public ImapConnectionType ConnectionType
        {
            get
            {
                return _connectionType;
            }

            private set
            {
                _connectionType = value;
                UseSSL = ConnectionType == ImapConnectionType.SSL_TLS ? true : false;
            }
        }

        private ImapConnectionType _connectionType;

        internal ImapConnection(ImapConnectionOptions options) :
            base(options.Host, options.Port, true, options.AllowInvalidRemoteCertificates)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

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
                await UpgradeToSecureConnection(cancellationToken);
            }

            var result = await ExecuteCommand(ImapCommands.Login(user, password), cancellationToken);
            IsAuthenticated = !result.Contains(ImapResponse.AUTHFAILED);
            return IsAuthenticated;
        }

        private async Task<bool> UpgradeToSecureConnection(CancellationToken cancellationToken)
        {
            var commandResult = await ExecuteCommand(ImapCommands.StartTLS(), cancellationToken);
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

        public async Task<bool> SelectFolder(string folder, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteCommand(ImapCommands.SelectFolder(folder), cancellationToken);

            //Double check, some servers sometimes include a last line with a & OK appending extra info when command fails
            return result.Contains(ImapResponse.OK) && !result.Contains(ImapResponse.ERROR);
        }

        public async Task<string> GetFolders(CancellationToken cancellationToken = default)
        {
            return await ExecuteCommand(ImapCommands.ListFolders(), cancellationToken);
        }
    }
}
