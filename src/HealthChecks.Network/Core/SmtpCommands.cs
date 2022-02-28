namespace HealthChecks.Network.Core
{
    public class SmtpCommands
    {
        public static string STARTTLS() => "STARTTLS\r\n";
        public static string EHLO(string host) => $"EHLO {host}\r\n";
        public static string AUTHLOGIN() => $"AUTH LOGIN\r\n";
    }

    public class SmtpResponse
    {
        public const string ACTION_OK = "250";
        public const string SERVICE_READY = "220";
        public const string AUTHENTICATION_SUCCESS = "235";
    }
}
