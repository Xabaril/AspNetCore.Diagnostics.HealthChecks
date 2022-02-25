using System;
using HealthChecks.UI.Image.Configuration;
using Microsoft.AspNetCore.Http;

namespace HealthChecks.UI.Image.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsAuthenticated(this HttpRequest request)
        {
            return request.Query.ContainsKey(PushServiceKeys.AuthParameter) &&
                request.Query[PushServiceKeys.AuthParameter] == Environment.GetEnvironmentVariable(PushServiceKeys.PushEndpointSecret);
        }
    }
}
