using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HealthChecks.Elasticsearch
{
    public class ElasticsearchOptions
    {
        public string Uri { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public X509Certificate Certificate { get; private set; }
        public bool AuthenticateWithBasicCredentials { get; private set; } = false;
        public bool AuthenticateWithCertificate { get; private set; } = false;
        public Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> CertificateValidationCallback { get; private set; }
        public TimeSpan? RequestTimeout { get; set; }
        public ElasticsearchOptions UseBasicAuthentication(string name, string password)
        {
            UserName = name ?? throw new ArgumentNullException(nameof(name));
            Password = password ?? throw new ArgumentNullException(nameof(password));

            Certificate = null;
            AuthenticateWithCertificate = false;
            AuthenticateWithBasicCredentials = true;
            return this;
        }
        public ElasticsearchOptions UseCertificate(X509Certificate certificate)
        {
            Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));

            UserName = string.Empty;
            Password = string.Empty;
            AuthenticateWithBasicCredentials = false;
            AuthenticateWithCertificate = true;
            return this;
        }
        public ElasticsearchOptions UseServer(string uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));

            return this;
        }
        public ElasticsearchOptions UseCertificateValidationCallback(Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> callback)
        {
            CertificateValidationCallback = callback;
            return this;
        }
    }
}