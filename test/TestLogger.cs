using Microsoft.Extensions.Logging;

public sealed class TestLogger : ILogger
{
    public List<(DateTime, string)> EventLog { get; } = new();
    public IDisposable
#if NET7_0_OR_GREATER
        ?
#endif
        BeginScope<TState>(TState state)
#if NET7_0_OR_GREATER
        where TState : notnull
#endif
        => default!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => EventLog.Add(new(DateTime.Now, formatter(state, exception)));
}
