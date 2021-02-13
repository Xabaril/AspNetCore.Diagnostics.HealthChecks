using HealthChecks.Network.Core;
using System;

namespace HealthChecks.Network
{
    public class SmtpHealthCheckOptions : SmtpConnectionOptions
    {
        internal (bool Login, (string, string) Account) AccountOptions { get; private set; }
        public void LoginWith(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException(nameof(userName));

            AccountOptions = (Login: true, Account: (userName, password));
        }
    }
}
