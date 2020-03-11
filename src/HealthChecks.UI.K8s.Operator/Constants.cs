using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.UI.K8s.Operator
{
    internal class Constants
    {
        //Kubernetes 1.17 allows Defaulting values - Next operators versions might use CRD default values instead of constant defaults
        public const string Group = "aspnetcore.ui";
        public const string Version = "v1";
        public const string Plural = "healthchecks";
        public const string PodName = "healthchecks-ui";
        public const string ImageName = "xabarilcoding/healthchecksui";
        public const string PushServicePath = "/healthchecks/push";
        public const string HealthCheckPathAnnotation = "HealthChecksPath";
        public const string HealthCheckSchemeAnnotation = "HealthChecksScheme";
        public const string DefaultPullPolicy = "Always";
        public const string DefaultServiceType = "ClusterIP";
        public const string DefaultUIPath = "/healthchecks";
        public const string DefaultPort = "80";
        public const string DefaultScheme = "http";
        public const string DefaultHealthPath = "health";
        public const string PushServiceAuthKey = "key";
    }
}
