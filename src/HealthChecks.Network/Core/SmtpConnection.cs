using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Network.Core
{
    internal class SmtpConnection : MailConnection
    {
        private readonly SmtpConnectionOptions _options;
        private SmtpConnectionType _connectionType;

        public SmtpConnectionType ConnectionType
        {
            get
            {
                return _connectionType;
            }

            private set
            {
                _connectionType = value;
                UseSSL = ConnectionType == SmtpConnectionType.SSL ? true : false;
            }
        }

        private bool ShouldUpgradeConnection => !UseSSL && _connectionType != SmtpConnectionType.PLAIN;

        public SmtpConnection(SmtpConnectionOptions options)
            : base(options.Host, options.Port, false, options.AllowInvalidRemoteCertificates)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            ConnectionType = _options.ConnectionType;
            ComputeDefaultValues();
        }

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

        public new async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            await base.ConnectAsync(cancellationToken);
            var result = await ExecuteCommand(SmtpCommands.EHLO(Host), cancellationToken);
            return result.Contains(SmtpResponse.ACTION_OK);
        }

        public async Task<bool> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            if (ShouldUpgradeConnection)
            {
                await UpgradeToSecureConnection(cancellationToken);
            }
            await ExecuteCommand(SmtpCommands.EHLO(Host), cancellationToken);
            await ExecuteCommand(SmtpCommands.AUTHLOGIN(), cancellationToken);
            await ExecuteCommand($"{ ToBase64(userName)}\r\n", cancellationToken);

            password = password?.Length > 0 ? ToBase64(password) : "";

            var result = await ExecuteCommand($"{password}\r\n", cancellationToken);
            return result.Contains(SmtpResponse.AUTHENTICATION_SUCCESS);
        }


        private async Task<bool> UpgradeToSecureConnection(CancellationToken cancellationToken)
        {
            var upgradeResult = await ExecuteCommand(SmtpCommands.STARTTLS(), cancellationToken);
            if (upgradeResult.Contains(SmtpResponse.SERVICE_READY))
            {
                UseSSL = true;
                _stream = GetStream();
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
}
