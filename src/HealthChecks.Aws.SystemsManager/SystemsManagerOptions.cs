using Amazon;
using Amazon.Runtime;

namespace HealthChecks.Aws.SystemsManager
{
    public class SystemsManagerOptions
    {
        public AWSCredentials? Credentials { get; set; }

        public RegionEndpoint? RegionEndpoint { get; set; }

        internal HashSet<string> _parameters = new HashSet<string>();

        internal IEnumerable<string> Parameters => _parameters;

        /// <summary>
        /// Add a Parameter to be checked
        /// </summary>
        /// <param name="parameter">The parameter to be checked</param>
        /// <returns><see cref="SystemsManagerOptions"/></returns>
        public SystemsManagerOptions AddParameter(string parameter)
        {
            _parameters.Add(parameter);

            return this;
        }
    }
}
