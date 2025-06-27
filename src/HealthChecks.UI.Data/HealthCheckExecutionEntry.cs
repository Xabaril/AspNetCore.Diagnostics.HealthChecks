using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using HealthChecks.UI.Core;

namespace HealthChecks.UI.Data;

public class HealthCheckExecutionEntry
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public UIHealthStatus Status { get; set; }

    public string? Description { get; set; }

    public TimeSpan Duration { get; set; }

    public List<string>? Tags { get; set; }

    // Ek alanlar 
    [Column("Data", TypeName = "nvarchar(max)")]
    public string DataJson { get; set; } = string.Empty;

    [NotMapped]
    public Dictionary<string, object>? Data
    {
        get => string.IsNullOrEmpty(DataJson)
            ? new Dictionary<string, object>()
            : JsonSerializer.Deserialize<Dictionary<string, object>>(DataJson);
        set => DataJson = JsonSerializer.Serialize(value);
    }
}
