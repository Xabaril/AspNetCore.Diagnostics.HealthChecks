using System;
using System.Buffers;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecks.Network.Core
{
    public class MailConnection : IDisposable
    {
        public int Port { get; protected set; }
        public string Host { get; protected set; }
        protected bool UseSSL { get; set; } = true;

        protected TcpClient _tcpClient;
        protected Stream _stream;
        protected Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> _validateRemoteCertificate = (o, c, ch, e) => true;
        private bool _disposed;
        private readonly bool _allowInvalidCertificates;

        public MailConnection(string host, int port, bool useSSL = true, bool allowInvalidCertificates = false)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            if (port == default) throw new ArgumentNullException(nameof(port));
            Port = port;
            UseSSL = useSSL;
            _allowInvalidCertificates = allowInvalidCertificates;
        }
        public async Task<bool> ConnectAsync()
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(Host, Port);

            _stream = GetStream();
            await ExecuteCommand(string.Empty);

            return _tcpClient.Connected;
        }
        protected Stream GetStream()
        {
            var stream = _tcpClient.GetStream();

            if (UseSSL)
            {
                var sslStream = GetSSLStream(stream);
                sslStream.AuthenticateAsClient(Host);
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
        protected async Task<string> ExecuteCommand(string command)
        {
            var buffer = Encoding.ASCII.GetBytes(command);
            await _stream.WriteAsync(buffer, 0, buffer.Length);

            var readBuffer = ArrayPool<byte>.Shared.Rent(512);
            int read = await _stream.ReadAsync(readBuffer, 0, readBuffer.Length);
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
}
