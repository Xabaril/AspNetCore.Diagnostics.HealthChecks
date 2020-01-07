using Google.Apis.Auth.OAuth2;

namespace HealthChecks.Gcp.CloudStorage
{
    public class CloudStorageOptions
    {
        /// <summary>
        /// This is the Project ID used in Google Cloud Platform to manage
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// This is the Bucket Name to monitor, if empty/null will try to monitor ProjectID level,
        /// but needs Permission at this level to query (by default is not given)
        /// </summary>
        public string Bucket { get; set; }

        /// <summary>
        /// The Google Credential to access the GCP (normally a Token, or a JSON, given through different ways
        /// Please initialize GoogleCredential (new GoogleCredential();) to obtain the passing constructors possible
        /// </summary>
        public GoogleCredential GoogleCredential { get; set; }
    }
}