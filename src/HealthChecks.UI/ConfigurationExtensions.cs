using HealthChecks.UI;
using HealthChecks.UI.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static Settings BindUISettings(this IConfiguration configuration, Settings settings)
        {
            configuration.GetSectionWithFallBack(
                    Keys.HEALTHCHECKSUI_SECTION_SETTING_KEY,
                    fallback: Keys.HEALTHCHECKSUI_OLD_SECTION_SETTING_KEY)
                .Bind(settings, c => c.BindNonPublicProperties = true);

            return settings;
        }

        public static IConfigurationSection GetSectionWithFallBack
            (this IConfiguration configuration, string section, string fallback)
        {
            IConfigurationSection configurationSection = configuration.GetSection(section);
            
            if (!configurationSection.Exists())
            {
                configurationSection = configuration.GetSection(fallback);
            }

            return configurationSection;
        }
    }
}