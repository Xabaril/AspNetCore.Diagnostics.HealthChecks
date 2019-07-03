using HealthChecks.UI.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static ICollection<UIStylesheet> GetCustomStylesheets(this UIResource resource, Options options)
        {
            List<UIStylesheet> styleSheets = new List<UIStylesheet>();
            
            if (!options.CustomStyleSheets.Any())
            {
                resource.Content = resource.Content.Replace("#customstylesheets#", string.Empty);
                return styleSheets;
            }

            foreach (var stylesheet in options.CustomStyleSheets)
            {
                styleSheets.Add(UIStylesheet.Create(options, stylesheet));
            }
            
            var htmlStyles = styleSheets.Select
                (s =>
            {
                var linkHref = options.UseRelativeResourcesPath ? s.ResourcePath.AsRelativeResource() : s.ResourcePath;
                return $"<link rel='stylesheet' href='{linkHref}'/>";
            });
            
            resource.Content = resource.Content.Replace("#customstylesheets#",
                string.Join("\n", htmlStyles));

            return styleSheets;
        }
    }    
}
