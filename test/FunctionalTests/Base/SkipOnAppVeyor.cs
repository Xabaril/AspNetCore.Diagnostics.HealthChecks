using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace FunctionalTests.Base
{

    /*
     * This class is a 'copy' of jbogard code for respawn
     * https://github.com/jbogard/Respawn/blob/master/Respawn.DatabaseTests/SkipOnAppVeyorAttribute.cs
     */

    public class SkipOnAppVeyorTestDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public SkipOnAppVeyorTestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            if (Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE")
            {
                return Enumerable.Empty<IXunitTestCase>();
            }

            return new[] { new XunitTestCase(_diagnosticMessageSink, TestMethodDisplay.Method, TestMethodDisplayOptions.All, testMethod) };
        }
    }



    [XunitTestCaseDiscoverer("FunctionalTests.Base.SkipOnAppVeyorTestDiscoverer", "FunctionalTests")]

    public class SkipOnAppVeyorAttribute : FactAttribute
    {

    }
}
