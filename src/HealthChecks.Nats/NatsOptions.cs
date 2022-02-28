namespace HealthChecks.Nats
{
    public class NatsOptions
    {
        /// <summary>
        /// A string containing the URL (or URLs) to the NATS Server.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The full path to a chained credentials file.
        /// </summary>
        public string CredentialsPath { get; set; }

        /// <summary>
        /// The path to a user's public JWT credentials.
        /// </summary>
        public string Jwt { get; set; }

        /// <summary>
        /// The path to a file for user's private Nkey seed.
        /// </summary>
        /// <remarks>
        /// <see href="https://docs.nats.io/running-a-nats-service/configuration/securing_nats/auth_intro/nkey_auth"/>
        /// </remarks>
        public string PrivateNKey { get; set; }
    }
}
