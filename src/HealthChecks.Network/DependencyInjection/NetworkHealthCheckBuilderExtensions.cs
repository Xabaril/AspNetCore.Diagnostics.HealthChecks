using HealthChecks.Network;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class NetworkHealthCheckBuilderExtensions
{
    private const string PING_NAME = "ping";
    private const string SFTP_NAME = "sftp";
    private const string FTP_NAME = "ftp";
    private const string DNS_NAME = "dns";
    private const string DNS_COUNT_NAME = "dns-host-count";
    private const string IMAP_NAME = "imap";
    private const string SMTP_NAME = "smtp";
    private const string TCP_NAME = "tcp";
    private const string SSL_NAME = "ssl";

    /// <summary>
    /// Add a health check for network ping.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure the ping parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ping' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddPingHealthCheck(
        this IHealthChecksBuilder builder,
        Action<PingHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new PingHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? PING_NAME,
            sp => new PingHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for network SFTP.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure the SFTP connection parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sftp' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSftpHealthCheck(
        this IHealthChecksBuilder builder,
        Action<SftpHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new SftpHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? SFTP_NAME,
            sp => new SftpHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }


    /// <summary>
    /// Add a health check for network SFTP.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="sftpConfigurationFactory">A factory to build the SftpConfiguration to use.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sftp' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSftpHealthCheck(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, SftpConfiguration> sftpConfigurationFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? SFTP_NAME,
            sp =>
            {
                var options = new SftpHealthCheckOptions();
                var sftpConfiguration = sftpConfigurationFactory.Invoke(sp);
                options.AddHost(sftpConfiguration);
                return new SftpHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for network FTP.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure the FTP connection parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ftp' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddFtpHealthCheck(
        this IHealthChecksBuilder builder,
        Action<FtpHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new FtpHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? FTP_NAME,
            sp => new FtpHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for network DNS solve.
    /// </summary>
    /// <remarks>
    /// Add host configurations using setup.ResolveHost(host).To(registrations);
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure DNS solve parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'dns' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddDnsResolveHealthCheck(
        this IHealthChecksBuilder builder,
        Action<DnsResolveOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default)
    {
        var options = new DnsResolveOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? DNS_NAME,
            sp => new DnsResolveHealthCheck(options),
            failureStatus,
            tags));
    }

    /// <summary>
    /// Add a Healthcheck to resolve a hostname and verify the number of resolved address is within the configured minimum and maximum
    /// </summary>
    /// <remarks>
    /// Add host configurations using setup.ResolveHost(host).To(registrations);
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">Action to add hosts and configure minimum and maximum resolved addresses.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'dns' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddDnsResolveHostCountHealthCheck(
        this IHealthChecksBuilder builder,
        Action<DnsResolveCountOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default)
    {
        var options = new DnsResolveCountOptions();
        setup?.Invoke(options);

        if (options.HostRegistrations.Count == 0)
            throw new ArgumentNullException(nameof(options), "No dns hosts have been registered");

        return builder.Add(new HealthCheckRegistration(
            name ?? DNS_COUNT_NAME,
            sp => new DnsResolveHostCountHealthCheck(options),
            failureStatus,
            tags));
    }

    /// <summary>
    /// Add a health check for network IMAP connections.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure IMAP connection parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'imap' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddImapHealthCheck(
        this IHealthChecksBuilder builder,
        Action<ImapHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = null)
    {
        var options = new ImapHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? IMAP_NAME,
            sp => new ImapHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for network SMTP connection.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure SMTP connection parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'smtp' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSmtpHealthCheck(
        this IHealthChecksBuilder builder,
        Action<SmtpHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new SmtpHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? SMTP_NAME,
            sp => new SmtpHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for network TCP connection.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure TCP connection parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'tcp' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddTcpHealthCheck(
        this IHealthChecksBuilder builder,
        Action<TcpHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new TcpHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? TCP_NAME,
            sp => new TcpHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for SSL connection and validate the certificate.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure Ssl connection parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'tcp' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSslHealthCheck(
        this IHealthChecksBuilder builder,
        Action<SslHealthCheckOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new SslHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? SSL_NAME,
            sp => new SslHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
