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

        if (nameItems.Length == 1 && nameItems[0] == "UnitTests")
        {
            // TODO: remove this condition after removing UnitTests project
            return;
        }

        var nameToFind = nameItems[1]; // 0 element always equals to "HealthChecks"
        if (nameToFind == "UI")
        {
            // TODO: Add API approval tests to UI projects and/or split UI tests into separate test projects
            return;
        }

        var asmForTest = dependencies.Select(dep => Assembly.Load(dep)).Where(asm => asm.GetTypes().Any(type => type.Name == "ApiMarker") && asm.GetName().Name!.Contains(nameToFind, StringComparison.InvariantCultureIgnoreCase)).Single();

        // https://github.com/PublicApiGenerator/PublicApiGenerator
        string publicApi = asmForTest.GeneratePublicApi(new ApiGeneratorOptions
        {
            IncludeAssemblyAttributes = false,
            AllowNamespacePrefixes = new[] { "Microsoft" }
        });

        var location = Assembly.GetExecutingAssembly().Location;
        var pathItems = location.Split(Path.DirectorySeparatorChar);
        var index = Array.IndexOf(pathItems, "test");
        Debug.Assert(index > 0 && index < pathItems.Length - 1);

        // See: https://docs.shouldly.org/documentation/equality/matchapproved
        // Note: If the AssemblyName.approved.txt file doesn't match the latest publicApi value,
        // this call will try to launch a diff tool to help you out but that can fail on
        // your machine if a diff tool isn't configured/setup.
        publicApi.ShouldMatchApproved(options => options.SubFolder(Path.Combine(".." /*_SHARED*/, pathItems[index + 1])).WithFilenameGenerator((testMethodInfo, discriminator, fileType, fileExtension) => $"{asmForTest.GetName().Name}.{fileType}.{fileExtension}"));
    }
}
