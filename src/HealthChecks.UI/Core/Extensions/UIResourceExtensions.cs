using HealthChecks.UI.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthChecks.UI.Core
{
    internal static class UIResourceExtensions
    {
        public static UIResource GetMainUI(this IEnumerable<UIResource> resources, Options options)
        {
            var resource = resources
                .FirstOrDefault(r => r.ContentType == ContentType.HTML && r.FileName == Keys.HEALTHCHECKSUI_MAIN_UI_RESOURCE);

            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_MAIN_UI_API_TARGET, options.ApiPath);

            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_WEBHOOKS_API_TARGET, options.WebhookPath);

            
            var isMultisegmentUI = options.UIPath.Count(c => c == '/') > 1;

            var resourcePath = isMultisegmentUI ? options.ResourcesPath : options.ResourcesPath.AsRelativeResource();
            
            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_RESOURCES_TARGET, resourcePath);

            return resource;
        }
    }
}
