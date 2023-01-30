using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Elasticsearch.Net;

namespace HealthChecks.Elasticsearch;

/// <summary>
/// Options for <see cref="ElasticsearchHealthCheck"/>.
/// </summary>
public class ElasticsearchOptions
{
    public string Uri { get; private set; } = null!;

    public string? UserName { get; private set; }

    public string? Password { get; private set; }

    public X509Certificate? Certificate { get; private set; }

    public ApiKeyAuthenticationCredentials? ApiKeyAuthenticationCredentials { get; set; }

    public bool AuthenticateWithBasicCredentials { get; private set; }

    public bool AuthenticateWithCertificate { get; private set; }

    public bool AuthenticateWithApiKey { get; private set; }

    public bool UseClusterHealthApi { get; set; }

    public Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool>? CertificateValidationCallback { get; private set; }

    public TimeSpan? RequestTimeout { get; set; }

    public ElasticsearchOptions UseBasicAuthentication(string name, string password)
    {
        UserName = Guard.ThrowIfNull(name);
        Password = Guard.ThrowIfNull(password);

        Certificate = null;
        AuthenticateWithApiKey = false;
        AuthenticateWithCertificate = false;
        AuthenticateWithBasicCredentials = true;
        return this;
    }

    public ElasticsearchOptions UseCertificate(X509Certificate certificate)
    {
        Certificate = Guard.ThrowIfNull(certificate);

        UserName = string.Empty;
        Password = string.Empty;
        AuthenticateWithApiKey = false;
        AuthenticateWithBasicCredentials = false;
        AuthenticateWithCertificate = true;
        return this;
    }

    public ElasticsearchOptions UseApiKey(ApiKeyAuthenticationCredentials apiKey)
    {
        ApiKeyAuthenticationCredentials = Guard.ThrowIfNull(apiKey);

        UserName = string.Empty;
        Password = string.Empty;
        Certificate = null;
        AuthenticateWithBasicCredentials = false;
        AuthenticateWithCertificate = false;
        AuthenticateWithApiKey = true;

        return this;
    }

    public ElasticsearchOptions UseServer(string uri)
    {
        Uri = Guard.ThrowIfNull(uri);

        return this;
    }

    public ElasticsearchOptions UseCertificateValidationCallback(Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> callback)
    {
        CertificateValidationCallback = callback;
        return this;
    }
}
