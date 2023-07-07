using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HealthChecks.SendGrid;

public class SendGridHealthCheck : IHealthCheck
{
    private const string MAIL_ADDRESS_NAME = "Health Check User";
    private const string MAIL_ADDRESS = "healthcheck@example.com";
    private const string SUBJECT = "Checking health is Fun";

    private readonly string _apiKey;
    private readonly IHttpClientFactory _httpClientFactory;

    public SendGridHealthCheck(string apiKey, IHttpClientFactory httpClientFactory)
    {
        _apiKey = Guard.ThrowIfNull(apiKey);
        _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(SendGridHealthCheckExtensions.NAME);

            var client = new SendGridClient(httpClient, _apiKey);
            var from = new EmailAddress(MAIL_ADDRESS, MAIL_ADDRESS_NAME);
            var to = new EmailAddress(MAIL_ADDRESS, MAIL_ADDRESS_NAME);
            var msg = MailHelper.CreateSingleEmail(from, to, SUBJECT, SUBJECT, null);
            msg.SetSandBoxMode(true);

            var response = await client.SendEmailAsync(msg, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new HealthCheckResult(context.Registration.FailureStatus,
                    $"Sending an email to SendGrid using the sandbox mode is not responding with 200 OK, the current status is {response.StatusCode}",
                    null,
                    new Dictionary<string, object>
                    {
                        { "responseStatusCode", (int)response.StatusCode }
                    });
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
