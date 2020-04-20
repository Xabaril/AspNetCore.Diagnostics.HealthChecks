namespace System
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string str) => string.IsNullOrEmpty(str);
        public static bool NotEmpty(this string str) => !IsEmpty(str);
    }
}
