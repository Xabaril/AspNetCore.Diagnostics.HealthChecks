using Xunit;

namespace FunctionalTests.Base
{
    [CollectionDefinition("execution")]
    public class CollectionExecutionFixture
        : ICollectionFixture<ExecutionFixture>
    {
    }
}
