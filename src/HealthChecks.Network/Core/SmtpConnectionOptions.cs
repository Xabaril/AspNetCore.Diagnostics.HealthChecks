namespace HealthChecks.Network.Core
{
    public class SmtpConnectionOptions
    {
        public string Host { get; set; } = null!;

        public int Port { get; set; }

        public bool AllowInvalidRemoteCertificates { get; set; }

        public SmtpConnectionType ConnectionType = SmtpConnectionType.AUTO;
    }
}
