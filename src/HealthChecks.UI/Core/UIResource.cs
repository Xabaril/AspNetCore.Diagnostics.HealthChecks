using System;

namespace HealthChecks.UI.Core
{
    internal class UIResource
    {
        public string Content { get; internal set; }
        public string ContentType { get; }
        public string FileName { get; }

        private UIResource(string fileName, string content, string contentType)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public static UIResource Create(string fileName, string content, string contentType)
        {
            return new UIResource(fileName, content, contentType);
        }
    }
}
