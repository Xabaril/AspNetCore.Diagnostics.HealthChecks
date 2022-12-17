using Microsoft.Extensions.Logging;

public sealed class TestLogger : ILogger
{
    public List<(DateTime, string)> EventLog { get; } = new();
    public IDisposable BeginScope<TState>(TState state) => default!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => EventLog.Add(new(DateTime.Now, formatter(state, exception)));
}
