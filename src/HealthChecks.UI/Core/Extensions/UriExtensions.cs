using System;

namespace HealthChecks.UI.Core.Extensions
{
    public static class UriExtensions
    {
        public static bool IsValidHealthCheckEndpoint(this Uri uri) =>
            uri.IsAbsoluteUri && !uri.IsFile && (uri.Scheme == "http" || uri.Scheme == "https");
    }
}