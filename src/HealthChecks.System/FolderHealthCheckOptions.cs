namespace HealthChecks.System;

/// <summary>
/// Options for <see cref="FolderHealthCheck"/>.
/// </summary>
public class FolderHealthCheckOptions
{
    public IList<string> Folders { get; set; } = new List<string>();
    public bool CheckAllFolders { get; set; }

    public FolderHealthCheckOptions AddFolder(string folder)
    {
        Folders.Add(folder);
        return this;
    }

    public FolderHealthCheckOptions WithCheckAllFolders()
    {
        CheckAllFolders = true;
        return this;
    }
}
