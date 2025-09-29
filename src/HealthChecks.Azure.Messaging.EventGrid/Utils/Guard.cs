namespace HealthChecks.Azure.Messaging.EventGrid;

internal static class Guard
{
    public static T ThrowIfNull<T>(T value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        return value;
    }
}