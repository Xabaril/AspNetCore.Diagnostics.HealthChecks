namespace HealthChecks.System;

/// <summary>
/// Options for <see cref="FileHealthCheck"/>.
/// </summary>
public class FileHealthCheckOptions
{
    public List<string> Files { get; } = new();
    public bool CheckAllFiles { get; set; }

    public FileHealthCheckOptions AddFile(string file)
    {
        Files.Add(file);
        return this;
    }

    public FileHealthCheckOptions WithCheckAllFiles()
    {
        CheckAllFiles = true;
        return this;
    }
}
