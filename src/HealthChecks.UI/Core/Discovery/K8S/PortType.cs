using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.UI.Core.Discovery.K8S
{    
    internal enum PortType
    {
        LoadBalancer,
        NodePort,
        ClusterIP
    }
}
