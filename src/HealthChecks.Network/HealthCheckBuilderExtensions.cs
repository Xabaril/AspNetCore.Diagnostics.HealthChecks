using HealthChecks.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string PING_NAME = "ping";
        const string SFTP_NAME = "sftp";
        const string FTP_NAME = "ftp";
        const string DNS_NAME = "dns";
        const string IMAP_NAME = "imap";
        const string SMTP_NAME = "smtp";


        public static IHealthChecksBuilder AddPingHealthCheck(this IHealthChecksBuilder builder, Action<PingHealthCheckOptions> setup)
        {
            var options = new PingHealthCheckOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
               PING_NAME,
               sp => new PingHealthCheck(options, sp.GetService<ILogger<PingHealthCheck>>()),
               null,
               new string[] { PING_NAME }));
        }
        public static IHealthChecksBuilder AddSftpHealthCheck(this IHealthChecksBuilder builder, Action<SftpHealthCheckOptions> setup)
        {
            var options = new SftpHealthCheckOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
               SFTP_NAME,
               sp => new SftpHealthCheck(options, sp.GetService<ILogger<SftpHealthCheck>>()),
               null,
               new string[] { SFTP_NAME }));
        }

        public static IHealthChecksBuilder AddFtpHealthCheck(this IHealthChecksBuilder builder, Action<FtpHealthCheckOptions> setup)
        {
            var options = new FtpHealthCheckOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
               FTP_NAME,
               sp => new FtpHealthCheck(options, sp.GetService<ILogger<FtpHealthCheck>>()),
               null,
               new string[] { FTP_NAME }));

        }

        public static IHealthChecksBuilder AddDnsResolveHealthCheck(this IHealthChecksBuilder builder, Action<DnsResolveOptions> setup)
        {
            var options = new DnsResolveOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
               DNS_NAME,
               sp => new DnsResolveHealthCheck(options, sp.GetService<ILogger<DnsResolveHealthCheck>>()),
               null,
               new string[] { DNS_NAME }));
        }

        public static IHealthChecksBuilder AddImapHealthCheck(this IHealthChecksBuilder builder, Action<ImapHealthCheckOptions> setup)
        {
            var options = new ImapHealthCheckOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
               IMAP_NAME,
               sp => new ImapHealthCheck(options, sp.GetService<ILogger<ImapHealthCheck>>()),
               null,
               new string[] { IMAP_NAME }));
        }

        public static IHealthChecksBuilder AddSmtpHealthCheck(this IHealthChecksBuilder builder, Action<SmtpHealthCheckOptions> setup)
        {
            var options = new SmtpHealthCheckOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
               SMTP_NAME,
               sp => new SmtpHealthCheck(options, sp.GetService<ILogger<SmtpHealthCheck>>()),
               null,
               new string[] { SMTP_NAME }));
        }
    }
}
