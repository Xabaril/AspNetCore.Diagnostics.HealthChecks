using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace HealthChecks.System.Tests.Seedwork
{
    public class Platform
    {
        public const string WINDOWS = "WINDOWS";
        public const string LINUX = "LINUX";
        public const string OSX = "OSX";
    }

    public class SkipOnPlatformTestDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public SkipOnPlatformTestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            var attribute = testMethod.Method.ToRuntimeMethod()
                .GetCustomAttributes(typeof(SkipOnPlatformAttribute), true).FirstOrDefault();

            if (attribute != null)
            {
                foreach (var platform in ((SkipOnPlatformAttribute)attribute).Platforms)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Create(platform)))
                    {
                        Console.WriteLine($"[{nameof(SkipOnPlatformTestDiscoverer)}] Target platform is {platform}, skipping test {testMethod.Method.Name}");
                        return Enumerable.Empty<IXunitTestCase>();
                    }
                }
            }

            return new[]
            {
                new XunitTestCase(_diagnosticMessageSink, TestMethodDisplay.Method, TestMethodDisplayOptions.All ,testMethod)
            };
        }
    }

    [XunitTestCaseDiscoverer("HealthChecks.System.Tests.Seedwork.SkipOnPlatformTestDiscoverer", "HealthChecks.System.Tests")]
    public class SkipOnPlatformAttribute : FactAttribute
    {
        public List<string> Platforms { get; } = new List<string>();

        public SkipOnPlatformAttribute(params string[] platforms)
        {
            Platforms.AddRange(platforms);
        }
    }
}
