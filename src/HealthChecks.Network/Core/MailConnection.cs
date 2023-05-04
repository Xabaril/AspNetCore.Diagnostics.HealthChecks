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

        _stream = await GetStreamAsync().ConfigureAwait(false);
        await ExecuteCommand(string.Empty).ConfigureAwait(false);

        return _tcpClient.Connected;
    }

    protected async Task<Stream> GetStreamAsync()
    {
        if (_tcpClient == null)
            throw new InvalidOperationException($"{nameof(ConnectAsync)} should be called first");

        var stream = _tcpClient.GetStream();

        if (UseSSL)
        {
            var sslStream = GetSSLStream(stream);
            await sslStream.AuthenticateAsClientAsync(Host).ConfigureAwait(false);
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

#pragma warning disable IDE1006 // Naming Styles
    protected async Task<string> ExecuteCommand(string command) //TODO: rename public API
#pragma warning restore IDE1006 // Naming Styles
    {
        if (_stream == null)
            throw new InvalidOperationException($"{nameof(ConnectAsync)} should be called first");

        var buffer = Encoding.ASCII.GetBytes(command);
        await _stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

        var readBuffer = ArrayPool<byte>.Shared.Rent(512);
        int read = await _stream.ReadAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false);
        var output = Encoding.UTF8.GetString(readBuffer);

        ArrayPool<byte>.Shared.Return(readBuffer);

        return output;
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
