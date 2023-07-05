using System.Buffers;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
#if !NET5_0_OR_GREATER
using HealthChecks.Network.Extensions;
#endif

namespace HealthChecks.Network.Core;

public class MailConnection : IDisposable
{
    public int Port { get; protected set; }
    public string Host { get; protected set; }
    protected bool UseSSL { get; set; } = true;

    protected TcpClient? _tcpClient;
    protected Stream? _stream;
    protected Func<object, X509Certificate?, X509Chain?, SslPolicyErrors, bool> _validateRemoteCertificate = (o, c, ch, e) => true;
    private bool _disposed;
    private readonly bool _allowInvalidCertificates;

    public MailConnection(string host, int port, bool useSSL = true, bool allowInvalidCertificates = false)
    {
        Host = Guard.ThrowIfNull(host);
        if (port == default)
            throw new ArgumentNullException(nameof(port));
        Port = port;
        UseSSL = useSSL;
        _allowInvalidCertificates = allowInvalidCertificates;
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        _tcpClient = new TcpClient();
#if NET5_0_OR_GREATER
        await _tcpClient.ConnectAsync(Host, Port, cancellationToken).ConfigureAwait(false);
#else
        await _tcpClient.ConnectAsync(Host, Port).WithCancellationTokenAsync(cancellationToken).ConfigureAwait(false);
#endif

        _stream = await GetStreamAsync(cancellationToken).ConfigureAwait(false);
        await ExecuteCommandAsync(string.Empty, cancellationToken).ConfigureAwait(false);

        return _tcpClient.Connected;
    }

    protected async Task<Stream> GetStreamAsync(CancellationToken cancellationToken)
    {
        if (_tcpClient == null)
            throw new InvalidOperationException($"{nameof(ConnectAsync)} should be called first");

        var stream = _tcpClient.GetStream();

        if (UseSSL)
        {
            var sslStream = GetSSLStream(stream);

#if NET5_0_OR_GREATER
            var clientAuthenticationOptions = new SslClientAuthenticationOptions
            {
                TargetHost = Host
            };
            await sslStream.AuthenticateAsClientAsync(clientAuthenticationOptions, cancellationToken).ConfigureAwait(false);
#else
            await sslStream.AuthenticateAsClientAsync(Host).WithCancellationTokenAsync(cancellationToken).ConfigureAwait(false);
#endif
            return sslStream;
        }
        else
        {
            return stream;
        }
    }

    protected SslStream GetSSLStream(Stream stream)
    {
        if (_allowInvalidCertificates)
        {
            return new SslStream(stream, true, new RemoteCertificateValidationCallback(_validateRemoteCertificate));
        }
        else
        {
            return new SslStream(stream);
        }
    }

    protected async Task<string> ExecuteCommandAsync(string command, CancellationToken cancellationToken)
    {
        if (_stream == null)
            throw new InvalidOperationException($"{nameof(ConnectAsync)} should be called first");

        var buffer = Encoding.ASCII.GetBytes(command);

#if NET5_0_OR_GREATER
        await _stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
#else
        await _stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
#endif

        var readBuffer = ArrayPool<byte>.Shared.Rent(512);
        try
        {

#if NET5_0_OR_GREATER
            int read = await _stream.ReadAsync(readBuffer, cancellationToken).ConfigureAwait(false);
#else
            int read = await _stream.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken).ConfigureAwait(false);
#endif

            return Encoding.UTF8.GetString(readBuffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(readBuffer);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }
        _disposed = true;
    }
}
