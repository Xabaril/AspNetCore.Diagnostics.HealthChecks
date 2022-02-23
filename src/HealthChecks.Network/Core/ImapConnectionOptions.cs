namespace HealthChecks.Network.Core
{
    public class ImapConnectionOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public ImapConnectionType ConnectionType { get; set; } = ImapConnectionType.AUTO;
        public bool AllowInvalidRemoteCertificates { get; set; }
    }
}
