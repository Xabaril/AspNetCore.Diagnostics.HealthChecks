namespace HealthChecks.SmbCifs
{
    public class SmbCifsExtendedOptions : SmbCifsOptions
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
        /// The Challenge of domain if needed
        /// </summary>
        public byte[] Challenge { get; set; }
        /// <summary>
        /// The Hash in Ansi
        /// </summary>
        public byte[] AnsiHash { get; set; }
        /// <summary>
        /// The Hash in Unicode
        /// </summary>
        public byte[] UnicodeHash { get; set; }
    }
}
