using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace HealthChecks.Npgsql.Tests.Functional;
public sealed class TestLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, TestLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, _ => new TestLogger());
    public void Dispose() => _loggers.Clear();
    public TestLogger GetLogger(string categoryName) => _loggers.GetValueOrDefault(categoryName);
}
