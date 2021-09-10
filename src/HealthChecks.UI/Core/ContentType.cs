using System;
using System.Collections.Generic;

namespace HealthChecks.UI.Core
{
    internal class ContentType
    {
        public const string JAVASCRIPT = "text/javascript";
        public const string CSS = "text/css";
        public const string HTML = "text/html";
        public const string PLAIN = "text/plain";

        public static Dictionary<string, string> supportedContent =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "js", JAVASCRIPT },
            { "html", HTML },
            { "css", CSS }
        };

        public static string FromExtension(string fileExtension)
            => supportedContent.TryGetValue(fileExtension, out var result) ? result : PLAIN;
    }
}
