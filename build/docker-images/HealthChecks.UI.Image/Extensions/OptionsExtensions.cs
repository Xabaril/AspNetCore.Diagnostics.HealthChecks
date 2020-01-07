using HealthChecks.UI.Configuration;
using HealthChecks.UI.Image.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthChecks.UI.Image.Extensions
{
    public static class OptionsExtensions
    {
        public static void ConfigureStylesheet(this Options options, IConfiguration configuration)
        {
            var uiStylesheet = configuration[UIKeys.UI_STYLESHEET];

            if (!string.IsNullOrEmpty(uiStylesheet))
            {
                options.AddCustomStylesheet(uiStylesheet);
            }
        }
        public static void ConfigurePaths(this Options options, IConfiguration configuration)
        {
            var uiPath = configuration[UIKeys.UI_PATH];
            var apiPath = configuration[UIKeys.UI_API_PATH];
            var resourcesPath = configuration[UIKeys.UI_RESOURCES_PATH];
            var webhooksPath = configuration[UIKeys.UI_WEBHOOKS_PATH];
            var relativePaths = configuration[UIKeys.UI_NO_RELATIVE_PATHS];

            bool disableRelativePaths = false;

            if (!string.IsNullOrEmpty(relativePaths))
            {
                bool.TryParse(relativePaths, out disableRelativePaths);
            }

            if (!string.IsNullOrEmpty(uiPath)) options.UIPath = uiPath;
            if (!string.IsNullOrEmpty(apiPath)) options.ApiPath = apiPath;
            if (!string.IsNullOrEmpty(resourcesPath)) options.ResourcesPath = resourcesPath;
            if (!string.IsNullOrEmpty(webhooksPath)) options.WebhookPath = webhooksPath;

            if (disableRelativePaths)
            {
                options.UseRelativeApiPath = false;
                options.UseRelativeResourcesPath = false;
                options.UseRelativeWebhookPath = false;
            }
        }
    }
}
