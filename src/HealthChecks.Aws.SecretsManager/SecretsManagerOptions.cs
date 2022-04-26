using Amazon;
using Amazon.Runtime;

namespace HealthChecks.Aws.SecretsManager
{
    public class SecretsManagerOptions
    {
        public AWSCredentials? Credentials { get; set; }

        public RegionEndpoint? RegionEndpoint { get; set; }


        internal HashSet<string> _secrets = new HashSet<string>();

        internal IEnumerable<string> Secrets => _secrets;

        /// <summary>
        /// Add an AWS Secrets Manager secret to be checked
        /// </summary>
        /// <param name="secretName">The secret to be checked</param>
        /// <returns><see cref="SecretsManagerOptions"/></returns>
        public SecretsManagerOptions AddSecret(string secretName)
        {
            _secrets.Add(secretName);

            return this;
        }
    }
}
