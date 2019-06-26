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

            var apiPath = options.UseRelativeApiPath ? options.ApiPath.AsRelativeResource() : options.ApiPath;

            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_MAIN_UI_API_TARGET, apiPath);


            var webhooksPath = options.UseRelativeWebhookPath ? options.WebhookPath.AsRelativeResource() : options.WebhookPath;

            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_WEBHOOKS_API_TARGET, webhooksPath);

            
            var resourcePath = options.UseRelativeResourcesPath ? options.ResourcesPath.AsRelativeResource() : options.ResourcesPath;
            
            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_RESOURCES_TARGET, resourcePath);

            return resource;
        }
    }
}
