using HealthChecks.Network;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Network
{
    public class network_registration_should : base_should
    {
        [Fact]
        public void add_ping_health_check_when_properly_configured()
        {
            ShouldPass("ping", typeof(PingHealthCheck), builder => builder.AddPingHealthCheck(_ => { }));
        }
        [Fact]
        public void add_named_ping_health_check_when_properly_configured()
        {
            ShouldPass("my-ping-1", typeof(PingHealthCheck), builder => builder.AddPingHealthCheck(_ => { }, name: "my-ping-1"));
        }
        [Fact]
        public void add_sftp_check_when_properly_configured()
        {
            ShouldPass("sftp", typeof(SftpHealthCheck), builder => builder.AddSftpHealthCheck(_ => { }));
        }
        [Fact]
        public void add_named_sftp_health_check_when_properly_configured()
        {
            ShouldPass("my-sftp-1", typeof(SftpHealthCheck), builder => builder.AddSftpHealthCheck(_ => { }, name: "my-sftp-1"));
        }
        [Fact]
        public void add_ftp_check_when_properly_configured()
        {
            ShouldPass("ftp", typeof(FtpHealthCheck), builder => builder.AddFtpHealthCheck(_ => { }));
        }
        [Fact]
        public void add_named_ftp_health_check_when_properly_configured()
        {
            ShouldPass("my-ftp-1", typeof(FtpHealthCheck), builder => builder.AddFtpHealthCheck(_ => { }, name: "my-ftp-1"));
        }
        [Fact]
        public void add_dns_check_when_properly_configured()
        {
            ShouldPass("dns", typeof(DnsResolveHealthCheck), builder => builder.AddDnsResolveHealthCheck(_ => { }));
        }
        [Fact]
        public void add_named_dns_health_check_when_properly_configured()
        {
            ShouldPass("my-dns-1", typeof(DnsResolveHealthCheck), builder => builder.AddDnsResolveHealthCheck(_ => { }, name: "my-dns-1"));
        }
        [Fact]
        public void add_imap_check_when_properly_configured()
        {
            ShouldPass("imap", typeof(ImapHealthCheck), builder => builder.AddImapHealthCheck(
                opt => { opt.Host = "the-host"; opt.Port = 111; }));
        }
        [Fact]
        public void add_named_imap_health_check_when_properly_configured()
        {
            ShouldPass("my-imap-1", typeof(ImapHealthCheck), builder => builder.AddImapHealthCheck(
                opt => { opt.Host = "the-host"; opt.Port = 111; }, name: "my-imap-1"));
        }
        [Fact]
        public void add_smpt_check_when_properly_configured()
        {
            ShouldPass("smtp", typeof(SmtpHealthCheck), builder => builder.AddSmtpHealthCheck(_ => { }));
        }
        [Fact]
        public void add_named_smtp_health_check_when_properly_configured()
        {
            ShouldPass("my-smtp-1", typeof(SmtpHealthCheck), builder => builder.AddSmtpHealthCheck(_ => { }, name: "my-smtp-1"));
        }
        [Fact]
        public void add_tcp_check_when_properly_configured()
        {
            ShouldPass("tcp", typeof(TcpHealthCheck), builder => builder.AddTcpHealthCheck(_ => { }));
        }
        [Fact]
        public void add_named_tcp_health_check_when_properly_configured()
        {
            ShouldPass("tcp-1", typeof(TcpHealthCheck), builder => builder.AddTcpHealthCheck(_ => { }, name: "tcp-1"));
        }
    }
}
