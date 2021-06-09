using Azure.Core;
using Microsoft.Rest;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests.HealthChecks.DependencyInjection.AzureDigitalTwin
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
