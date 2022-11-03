namespace HealthChecks.UI.Core
{
    internal class UIResource
    {
        public string Content { get; internal set; }
        public string ContentType { get; }
        public string FileName { get; }

        private UIResource(string fileName, string content, string contentType)
        {
            Content = Guard.ThrowIfNull(content);
            ContentType = Guard.ThrowIfNull(contentType);
            FileName = Guard.ThrowIfNull(fileName);
        }

        public static UIResource Create(string fileName, string content, string contentType)
        {
            return new UIResource(fileName, content, contentType);
        }
    }
}
