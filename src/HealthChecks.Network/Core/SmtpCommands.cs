namespace HealthChecks.Network.Core;

public class SmtpCommands
{
    public static string STARTTLS() => "STARTTLS\r\n";
    public static string EHLO(string host) => $"EHLO {host}\r\n";
    public static string AUTHLOGIN() => $"AUTH LOGIN\r\n";
}
