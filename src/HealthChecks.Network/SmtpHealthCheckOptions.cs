using HealthChecks.Network.Core;

namespace HealthChecks.Network;

public class SmtpHealthCheckOptions : SmtpConnectionOptions
{
    internal (bool Login, (string, string) Account) AccountOptions { get; private set; }
    public void LoginWith(string userName, string password)
    {
        Guard.ThrowIfNull(userName, true);

        AccountOptions = (Login: true, Account: (userName, password));
    }
}
