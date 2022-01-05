namespace HealthChecks.Nats
{
    public class NatsOptions
    {
        /// <summary>
        ///  A string containing the URL (or URLs) to the NATS Server.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The full path to a chained credentials file.
        /// </summary>
        public string CredentialsPath { get; set; }

        /// <summary>
        /// The path to a user's public JWT credentials.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string Jwt { get; set; }

        /// <summary>
        /// The path to a file for user user's private Nkey seed.
        /// </summary>
        public string PrivateNKey { get; set; }
    }
}
