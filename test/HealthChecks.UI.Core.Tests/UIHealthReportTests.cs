namespace HealthChecks.UI.Core.Tests;
public class UIHealthReportTests
{
    [Fact]
    public void should_create_form_with_obfuscated_exception_when_exception_message_is_defined()
    {
        var healthReportKey = "Health Check with Exception";
        var entries = new Dictionary<string, HealthReportEntry>
        {
            { "Health Check with Exception", new HealthReportEntry(HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(1), new Exception("Custom Exception"), null) }
        };
        var report = new HealthReport(entries, TimeSpan.FromSeconds(1));
        var exceptionMessage = "Exception Occurred.";
        var form = UIHealthReport.CreateFrom(report, _ => exceptionMessage);

        form.Entries.ShouldHaveSingleItem();
        var reportEntry = form.Entries[healthReportKey];
        reportEntry.Exception.ShouldBe(exceptionMessage);
    }
}
