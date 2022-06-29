namespace System
{
    public static class StringExtensions
    {
        public static string AsRelativeResource(this string resourcePath)
        {
            return resourcePath.StartsWith("/") ? resourcePath.Substring(1) : resourcePath;
        }
    }
}
