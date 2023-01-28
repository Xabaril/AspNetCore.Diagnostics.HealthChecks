namespace HealthChecks.Network.Tests.DependencyInjection
{
    public class network_registration_should
    {
        [Fact]
        public void add_ping_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddPingHealthCheck(_ => { });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("ping");
            check.ShouldBeOfType<PingHealthCheck>();
        }
        [Fact]
        public void add_named_ping_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddPingHealthCheck(_ => { }, name: "my-ping-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-ping-1");
            check.ShouldBeOfType<PingHealthCheck>();
        }
        [Fact]
        public void add_sftp_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSftpHealthCheck(_ => { });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("sftp");
            check.ShouldBeOfType<SftpHealthCheck>();
        }
        [Fact]
        public void add_named_sftp_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSftpHealthCheck(_ => { }, name: "my-sftp-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-sftp-1");
            check.ShouldBeOfType<SftpHealthCheck>();
        }
        [Fact]
        public void add_ftp_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddFtpHealthCheck(_ => { });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("ftp");
            check.ShouldBeOfType<FtpHealthCheck>();
        }
        [Fact]
        public void add_named_ftp_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddFtpHealthCheck(_ => { }, name: "my-ftp-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-ftp-1");
            check.ShouldBeOfType<FtpHealthCheck>();
        }
        [Fact]
        public void add_dns_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDnsResolveHealthCheck(_ => { });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("dns");
            check.ShouldBeOfType<DnsResolveHealthCheck>();
        }
        [Fact]
        public void add_named_dns_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDnsResolveHealthCheck(_ => { }, name: "my-dns-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-dns-1");
            check.ShouldBeOfType<DnsResolveHealthCheck>();
        }
        [Fact]
        public void add_imap_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddImapHealthCheck(opt => { opt.Host = "the-host"; opt.Port = 111; });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("imap");
            check.ShouldBeOfType<ImapHealthCheck>();
        }
        [Fact]
        public void add_named_imap_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddImapHealthCheck(opt => { opt.Host = "the-host"; opt.Port = 111; }, name: "my-imap-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-imap-1");
            check.ShouldBeOfType<ImapHealthCheck>();
        }
        [Fact]
        public void add_smpt_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSmtpHealthCheck(_ => { });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("smtp");
            check.ShouldBeOfType<SmtpHealthCheck>();
        }
        [Fact]
        public void add_named_smtp_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSmtpHealthCheck(_ => { }, name: "my-smtp-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-smtp-1");
            check.ShouldBeOfType<SmtpHealthCheck>();
        }

        [Fact]
        public void add_named_tcp_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddTcpHealthCheck(_ => { }, name: "tcp-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("tcp-1");
            check.ShouldBeOfType<TcpHealthCheck>();
        }

        [Fact]
        public void add_ssl_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSslHealthCheck(options => options.AddHost("the-host"));

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("ssl");
            check.ShouldBeOfType<SslHealthCheck>();
        }

        [Fact]
        public void add_named_ssl_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddSslHealthCheck(options => options.AddHost("the-host", port: 111, checkLeftDays: 120), name: "ssl-1");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("ssl-1");
            check.ShouldBeOfType<SslHealthCheck>();
        }
    }
}
