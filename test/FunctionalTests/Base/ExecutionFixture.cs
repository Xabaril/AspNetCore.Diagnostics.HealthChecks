using System;

namespace FunctionalTests.Base
{
    public class ExecutionFixture
    {
        public bool IsAppVeyorExecution { get; private set; }

        public ExecutionFixture()
        {
            IsAppVeyorExecution = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";
        }
    }
}
