using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.UI.Core.Extensions
{
    public static class UIResourceExtensions
    {
        public static string AsRelativeResource(this string resourcePath)
        {
            return resourcePath.StartsWith("/") ? resourcePath.Substring(1) : resourcePath;
        }
    }
}
