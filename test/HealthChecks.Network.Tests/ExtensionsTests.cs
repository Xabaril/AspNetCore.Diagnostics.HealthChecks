using System.Text;
using HealthChecks.Network.Extensions;

namespace HealthChecks.Network.Tests;

public class ExtensionsTests
{
    [Theory]
    [InlineData("abcdef", "", true)]
    [InlineData("abcdef", "cd", true)]
    [InlineData("abcdef", "ef", true)]
    [InlineData("abcdef", "cdef", true)]
    [InlineData("abcdef", "abcdefg", false)]
    [InlineData("abcdef", "k", false)]
    [InlineData("abcdef", "ee", false)]
    [InlineData("abcdef", "ff", false)]
    public void ContainsArray(string source, string segment, bool expected)
    {
        Encoding.UTF8.GetBytes(source).ContainsArray(Encoding.UTF8.GetBytes(segment)).ShouldBe(expected);
    }
}
