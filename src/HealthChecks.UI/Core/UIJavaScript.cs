using HealthChecks.UI.Configuration;

namespace HealthChecks.UI.Core;

public class UIJavaScript
{
    private const string JAVASCRIPT_PATH = "js";
    public string FileName { get; }
    public byte[] Content { get; }
    public string ResourcePath { get; }

    private UIJavaScript(Options options, string filePath)
    {
        FileName = Path.GetFileName(filePath);
        Content = File.ReadAllBytes(filePath);
        ResourcePath = $"{options.ResourcesPath}/{JAVASCRIPT_PATH}/{FileName}";
    }

    public static UIJavaScript Create(Options options, string filePath)
    {
        return new UIJavaScript(options, filePath);
    }
}
