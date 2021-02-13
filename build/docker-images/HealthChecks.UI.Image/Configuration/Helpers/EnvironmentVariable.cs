using System;

namespace HealthChecks.UI.Image.Configuration.Helpers
{
    public class EnvironmentVariable
    {
        public static string GetValue(string variable) =>
            Environment.GetEnvironmentVariable(variable);

        public static bool HasValue(string variable) =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variable));
    }
}