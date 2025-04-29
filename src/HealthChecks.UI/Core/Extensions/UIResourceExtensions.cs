using HealthChecks.UI.Configuration;

namespace HealthChecks.UI.Core;

internal static class UIResourceExtensions
{
    public static UIResource GetMainUI(this IEnumerable<UIResource> resources, Options options)
    {
        var resource = resources
            .First(r => r.ContentType == ContentType.HTML && r.FileName == Keys.HEALTHCHECKSUI_MAIN_UI_RESOURCE);

        var apiPath = options.UseRelativeApiPath ? options.ApiPath.AsRelativeResource() : options.ApiPath;

        resource.Content = resource.Content
            .Replace(Keys.HEALTHCHECKSUI_MAIN_UI_API_TARGET, apiPath);

        var settingsPath = $"{apiPath}/{Keys.HEALTHCHECKS_SETTINGS_ENDPOINT}";

        resource.Content = resource.Content.Replace(Keys.HEALTHCHECKSUI_SETTINGS_ENDPOINT_TARGET, settingsPath);

        var webhooksPath = options.UseRelativeWebhookPath ? options.WebhookPath.AsRelativeResource() : options.WebhookPath;

        resource.Content = resource.Content
            .Replace(Keys.HEALTHCHECKSUI_WEBHOOKS_API_TARGET, webhooksPath);

        var resourcePath = options.UseRelativeResourcesPath ? options.ResourcesPath.AsRelativeResource() : options.ResourcesPath;

        resource.Content = resource.Content
            .Replace(Keys.HEALTHCHECKSUI_RESOURCES_TARGET, resourcePath);

        resource.Content = resource.Content
            .Replace(Keys.HEALTHCHECKSUI_ASIDEMENUEOPENED_TARGET, options.AsideMenuOpened.ToString().ToLower());

        resource.Content = resource.Content
            .Replace(Keys.HEALTHCHECKSUI_PAGE_TITLE, options.PageTitle);

        return resource;
    }

    public static ICollection<UIStylesheet> GetCustomStylesheets(this UIResource resource, Options options)
    {
        var styleSheets = new List<UIStylesheet>();

        if (options.CustomStylesheets.Count == 0)
        {
            resource.Content = resource.Content.Replace(Keys.HEALTHCHECKSUI_STYLESHEETS_TARGET, string.Empty);
            return styleSheets;
        }

        foreach (var stylesheet in options.CustomStylesheets)
        {
            styleSheets.Add(UIStylesheet.Create(options, stylesheet));
        }

        var htmlStyles = styleSheets.Select
            (s =>
        {
            var linkHref = options.UseRelativeResourcesPath ? s.ResourcePath.AsRelativeResource() : s.ResourcePath;
            return $"<link rel='stylesheet' href='{linkHref}'/>";
        });

        resource.Content = resource.Content.Replace(Keys.HEALTHCHECKSUI_STYLESHEETS_TARGET,
            string.Join("\n", htmlStyles));

        return styleSheets;
    }

    public static ICollection<UIJavaScript> GetCustomJavaScripts(this UIResource resource, Options options)
    {
        var javaScripts = new List<UIJavaScript>();

        if (options.CustomJavaScripts.Count == 0)
        {
            resource.Content = resource.Content.Replace(Keys.HEALTHCHECKSUI_JAVASCRIPTS_TARGET, string.Empty);
            return javaScripts;
        }

        foreach (var javaScript in options.CustomJavaScripts)
        {
            javaScripts.Add(UIJavaScript.Create(options, javaScript));
        }

        var htmlScripts = javaScripts.Select
            (s =>
            {
                var linkHref = options.UseRelativeResourcesPath ? s.ResourcePath.AsRelativeResource() : s.ResourcePath;
                return $"<script type='text/javascript' src='{linkHref}'></script>";

            });

        resource.Content = resource.Content.Replace(Keys.HEALTHCHECKSUI_JAVASCRIPTS_TARGET,
            string.Join("\n", htmlScripts));

        return javaScripts;
    }
}
