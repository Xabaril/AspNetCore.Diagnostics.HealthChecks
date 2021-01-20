using FluentAssertions;
using HealthChecks.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Network
{
    public class network_registration_should
    {
        [Fact]
        public void add_ping_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddPingHealthCheck(_ => { });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("ping");
            check.GetType().Should().Be(typeof(PingHealthCheck));
        }
        [Fact]
        public void add_named_ping_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddPingHealthCheck(_ => { }, name: "my-ping-1");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-ping-1");
            check.GetType().Should().Be(typeof(PingHealthCheck));
        }
        [Fact]
        public void add_sftp_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSftpHealthCheck(_ => { });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("sftp");
            check.GetType().Should().Be(typeof(SftpHealthCheck));
        }
        [Fact]
        public void add_named_sftp_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSftpHealthCheck(_ => { }, name: "my-sftp-1");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-sftp-1");
            check.GetType().Should().Be(typeof(SftpHealthCheck));
        }
        [Fact]
        public void add_ftp_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddFtpHealthCheck(_ => { });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("ftp");
            check.GetType().Should().Be(typeof(FtpHealthCheck));
        }
        [Fact]
        public void add_named_ftp_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddFtpHealthCheck(_ => { }, name: "my-ftp-1");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-ftp-1");
            check.GetType().Should().Be(typeof(FtpHealthCheck));
        }
        [Fact]
        public void add_dns_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDnsResolveHealthCheck(_ => { });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("dns");
            check.GetType().Should().Be(typeof(DnsResolveHealthCheck));
        }
        [Fact]
        public void add_named_dns_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDnsResolveHealthCheck(_ => { }, name: "my-dns-1");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-dns-1");
            check.GetType().Should().Be(typeof(DnsResolveHealthCheck));
        }
        [Fact]
        public void add_imap_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddImapHealthCheck(opt => { opt.Host = "the-host"; opt.Port = 111; });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("imap");
            check.GetType().Should().Be(typeof(ImapHealthCheck));
        }
        [Fact]
        public void add_named_imap_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddImapHealthCheck(opt => { opt.Host = "the-host"; opt.Port = 111; }, name: "my-imap-1");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-imap-1");
            check.GetType().Should().Be(typeof(ImapHealthCheck));
        }
        [Fact]
        public void add_smpt_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSmtpHealthCheck(_ => { });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("smtp");
            check.GetType().Should().Be(typeof(SmtpHealthCheck));
        }
        [Fact]
        public void add_named_smtp_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSmtpHealthCheck(_ => { }, name: "my-smtp-1");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-smtp-1");
            check.GetType().Should().Be(typeof(SmtpHealthCheck));
        }

        [Fact]
        public void add_named_tcp_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddTcpHealthCheck(_ => { }, name: "tcp-1");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("tcp-1");
            check.GetType().Should().Be(typeof(TcpHealthCheck));
        }
    }
}
