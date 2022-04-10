namespace HealthChecks.System
{
    /// <summary>
    /// Options for <see cref="FolderHealthCheck"/>.
    /// </summary>
    public class FolderHealthCheckOptions
    {
        public FolderHealthCheckOptions()
        {
            Folders = new List<string>();
        }

        public IList<string> Folders { get; set; } = new();
        
        public FolderHealthCheckOptions AddFolder(string folder)
        {
            Folders.Add(folder);
            return this;
        }
    }
}
