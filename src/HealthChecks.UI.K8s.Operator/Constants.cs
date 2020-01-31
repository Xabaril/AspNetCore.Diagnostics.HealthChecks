using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.UI.K8s.Operator
{
    internal class Constants
    {
        public const string Group = "aspnetcore.ui";
        public const string Version = "v1";
        public const string Plural = "healthchecks";
        public const string PodName = "healthchecks-ui";
        public const string DockerImage = "xabarilcoding/healthchecksui";
        public const string UIDefaultPath = "/healthchecks";
        public const string PushServicePath = "/healthchecks/push";
        public const string ServicesLabel = "servicesLabel";
        public const string ServicesHealthPathLabel = "servicesHealthPathLabel";
        public const string ServicesSchemeLabel = "servicesSchemeLabel";
    }
}
