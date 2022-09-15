using Microsoft.Extensions.Logging;

namespace HealthChecks.Npgsql.Tests.Functional;

public sealed class TestLogger : ILogger
{
    public readonly List<(DateTime, string)> _eventLog = new();
    public IDisposable BeginScope<TState>(TState state) => default!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => _eventLog.Add(new(DateTime.Now, formatter(state, exception)));
}
