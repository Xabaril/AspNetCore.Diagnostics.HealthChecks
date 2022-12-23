using Azure.Core;
using Microsoft.Rest;

namespace HealthChecks.AzureDigitalTwin.Tests
{
    internal class MockTokenCredentials : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    internal class MockServiceClientCredentials : ServiceClientCredentials
    {
        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            throw new NotImplementedException();
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
