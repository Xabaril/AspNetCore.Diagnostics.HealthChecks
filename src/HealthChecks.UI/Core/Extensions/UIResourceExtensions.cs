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
                .Where(r => r.ContentType == ContentType.HTML && r.FileName == Keys.HEALTHCHECKSUI_MAIN_UI_RESOURCE)
                .FirstOrDefault();

            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_MAIN_UI_API_TARGET, options.ApiPath.AsRelativeResource());

            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_WEBHOOKS_API_TARGET, options.WebhookPath.AsRelativeResource());

            resource.Content = resource.Content
                .Replace(Keys.HEALTHCHECKSUI_RESOURCES_TARGET,options.ResourcesPath.AsRelativeResource());

            return resource;
        }
    }
}
