namespace HealthChecks.SmbCifs
{
    public class SmbCifsBasicOptions : SmbCifsOptions
    {
        /// <summary>
        /// The Domain to reach
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// The UserName used for domain authentication/permission
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The User Password for domain authentication/permission
        /// </summary>
        public string UserPassword { get; set; }
    }
}
