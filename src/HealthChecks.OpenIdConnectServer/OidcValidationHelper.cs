namespace HealthChecks.IdSvr;

internal static class OidcValidationHelper
{
    public static void ValidateValue(string value, string metadata)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(GetMissingValueExceptionMessage(metadata));
        }
    }

    public static void ValidateRequiredValues(IEnumerable<string> values, string metadata, IEnumerable<string> requiredValues)
    {
        if (values == null || !AnyValueContains(values, requiredValues))
        {
            throw new ArgumentException(GetMissingRequiredValuesExceptionMessage(metadata, requiredValues));
        }
    }

    private static bool AnyValueContains(IEnumerable<string> values, IEnumerable<string> requiredValues) =>
        values.Any(v => requiredValues.Contains(v));

    private static string GetMissingValueExceptionMessage(string value) =>
        $"Invalid discover response - '{value}' must be set!";

    private static string GetMissingRequiredValuesExceptionMessage(string value, IEnumerable<string> requiredValues) =>
        $"Invalid discover response - '{value}' must contain {string.Join(",", requiredValues)}!";
}
