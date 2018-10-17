using HealthChecks.UI.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace HealthChecks.UI.Core
{
    internal static class UIResourceExtensions
    {
        public static IEnumerable<UIResource> WithContentType(this IEnumerable<UIResource> resources, string contentType)
        {
            return resources.Where(r => r.ContentType == contentType);
        }           
       
        public static UIResource GetMainUI(this IEnumerable<UIResource> resources, Options options)
        {
            var resource = resources
                .WithContentType(ContentType.HTML)
                .FirstOrDefault(r => r.FileName == Keys.HEALTHCHECKSUI_MAIN_UI_RESOURCE);

            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_MAIN_UI_API_TARGET, options.ApiPath);

            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_WEBHOOKS_API_TARGET, options.WebhookPath);

            return resource;
        }
    }
}
