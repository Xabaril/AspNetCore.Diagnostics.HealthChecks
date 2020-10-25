using System;
using Amazon;
using Amazon.Runtime;

namespace HealthChecks.DynamoDb
{
    public class DynamoDBOptions
    {
        public AWSCredentials Credentials { get; set; }
        
        [Obsolete("Specify the access key and secret as a BasicCredential to the Credentials Field instead")]
        public string AccessKey { get; set;}
        
        [Obsolete("Specify the access key and secret as a BasicCredential to the Credentials Field instead")]
        public string SecretKey { get; set; }
        public RegionEndpoint RegionEndpoint { get; set; }
    }
}
