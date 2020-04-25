using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HealthChecks.SendGrid
{
    public class SendGridHealthCheck : IHealthCheck
    {
        private const string MailAddressName = "Health Check User";
        private const string MailAddress = "healthcheck@example.com";
        private const string Subject = "Checking health is Fun";
        private readonly string _apiKey;
        private readonly Func<HttpClient> _httpClientFactory;

        public SendGridHealthCheck(string apiKey, Func<HttpClient> httpClientFactory)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory();
                var client = new SendGridClient(httpClient, _apiKey);
                var from = new EmailAddress(MailAddress, MailAddressName);
                var to = new EmailAddress(MailAddress, MailAddressName);
                var msg = MailHelper.CreateSingleEmail(from, to, Subject, Subject, null);
                msg.SetSandBoxMode(true);
                var response = await client.SendEmailAsync(msg, cancellationToken);

                HealthCheckResult result;
                var isHealthy = response.StatusCode == HttpStatusCode.OK;
                if (isHealthy)
                {
                    result = HealthCheckResult.Healthy();
                }
                else
                {
                    result = new HealthCheckResult(
                        context.Registration.FailureStatus, 
                        $"Sending an email to SendGrid using the sandbox mode is not responding with 200 OK, the current status is {response.StatusCode}", 
                        null,
                        new Dictionary<string, object>
                        {
                            { "responseStatusCode", (int)response.StatusCode }
                        });
                }

                return result;
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
