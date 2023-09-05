namespace HealthChecks.System;

public class FileHealthCheckOptions
{
    internal List<string> Files { get; } = new();
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
