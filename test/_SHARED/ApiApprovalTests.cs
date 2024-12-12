using System.Diagnostics;
using System.Reflection;
using PublicApiGenerator;

public class ApiApprovalTests
{
    [Fact]
    [Trait("Category", "API Approval")]
    public void public_api_should_not_change_unintentionally()
    {
        var currentAsm = Assembly.GetExecutingAssembly();
        var dependencies = currentAsm.GetReferencedAssemblies();
        var nameItems = currentAsm.GetName().Name!.Split('.');

        Debug.Assert(nameItems.Last() == "Tests");

        var nameToFind = string.Join(".", nameItems.SkipLast(1));
        var asmForTest = dependencies
            .Select(Assembly.Load)
            .Where(a => !string.Equals(a.FullName, "Microsoft.Data.SqlClient, Version=5.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5", StringComparison.OrdinalIgnoreCase)) // https://github.com/dotnet/SqlClient/issues/1930#issuecomment-1814595368
            .Where(asm => asm.GetTypes().Any(type => type.Name == "ApiMarker") && asm.GetName().Name!.Equals(nameToFind, StringComparison.InvariantCultureIgnoreCase))
            .Single();

        // https://github.com/PublicApiGenerator/PublicApiGenerator
        string publicApi = asmForTest.GeneratePublicApi(new()
        {
            IncludeAssemblyAttributes = false,
            AllowNamespacePrefixes = ["Microsoft"]
        });

        var location = Assembly.GetExecutingAssembly().Location;
        var pathItems = location.Split(Path.DirectorySeparatorChar);
        var index = Array.IndexOf(pathItems, "test");
        Debug.Assert(index > 0 && index < pathItems.Length - 1);

        // See: https://docs.shouldly.org/documentation/equality/matchapproved
        // Note: If the AssemblyName.approved.txt file doesn't match the latest publicApi value,
        // this call will try to launch a diff tool to help you out but that can fail on
        // your machine if a diff tool isn't configured/setup.
        publicApi.ShouldMatchApproved(options => options.SubFolder(Path.Combine(".." /*_SHARED*/, pathItems[index + 1])).WithFilenameGenerator((_, _, fileType, fileExtension) => $"{asmForTest.GetName().Name}.{fileType}.{fileExtension}"));
    }
}
