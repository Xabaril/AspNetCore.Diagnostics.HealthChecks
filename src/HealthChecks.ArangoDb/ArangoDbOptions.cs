namespace HealthChecks.ArangoDb;

public class ArangoDbOptions
{
    public string HostUri { get; set; } = null!;

    public string Database { get; set; } = null!;

    public bool IsGenerateJwtTokenBasedOnUserNameAndPassword { get; set; } = false;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string JwtToken { get; set; } = string.Empty;
}
