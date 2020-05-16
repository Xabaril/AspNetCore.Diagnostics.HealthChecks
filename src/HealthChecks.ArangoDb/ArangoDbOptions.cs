namespace HealthChecks.ArangoDb
{
    public class ArangoDbOptions
    {
        public string HostUri { get; set; }
        public string Database { get; set; }
        public bool IsGenerateJwtTokenBasedOnUserNameAndPassword { get; set; } = false;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;
    }
}