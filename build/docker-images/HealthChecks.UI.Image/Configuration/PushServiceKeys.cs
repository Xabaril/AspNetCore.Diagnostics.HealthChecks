using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthChecks.UI.Image.Configuration
{
    public class PushServiceKeys
    {
        public const string Enabled = "enable_push_endpoint";
        public const string PushEndpointSecret = "push_endpoint_secret";
        public const int ServiceAdded = 0;
        public const int ServiceUpdated = 1;
        public const int ServiceRemoved = 2;
        public const string AuthParameter = "key";
    }
}
