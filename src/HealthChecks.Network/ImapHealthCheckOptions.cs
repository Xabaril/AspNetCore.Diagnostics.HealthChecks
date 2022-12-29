using HealthChecks.Network.Core;

namespace HealthChecks.Network
{
    public class ImapHealthCheckOptions : ImapConnectionOptions
    {
        internal (bool Login, (string, string) Account) AccountOptions { get; private set; }
        internal (bool CheckFolder, string FolderName) FolderOptions { get; private set; }

        public void LoginWith(string userName, string password)
        {
            Guard.ThrowIfNull(userName, true);
            Guard.ThrowIfNull(password, true);

            AccountOptions = (Login: true, Account: (userName, password));
        }

        public void CheckFolderExists(string inboxName)
        {
            FolderOptions = (true, inboxName);
        }
    }
}
