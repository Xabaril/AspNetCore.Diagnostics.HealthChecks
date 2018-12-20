using System;

namespace HealthChecks.Elasticsearch
{
    public class ElasticsearchOptions
    {
        public string Uri { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public bool AuthenticateWithBasicCredentials { get; private set; } = false;
        
        public ElasticsearchOptions UseBasicAuthentication(string name, string password)
        {
            UserName = name ?? throw new ArgumentNullException(nameof(name));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            AuthenticateWithBasicCredentials = true;

            return this;
        }
        public ElasticsearchOptions UseServer(string uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));

            return this;
        }
    }
}