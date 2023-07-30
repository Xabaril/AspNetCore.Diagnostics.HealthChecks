namespace HealthChecks.UI.K8s.Operator;

internal class Constants
{
    //Kubernetes 1.17 allows Defaulting values - Next operators versions might use CRD default values instead of constant defaults
    public const string GROUP = "aspnetcore.ui";
    public const string VERSION = "v1";
    public const string PLURAL = "healthchecks";
    public const string POD_NAME = "healthchecks-ui";
    public const string IMAGE_NAME = "xabarilcoding/healthchecksui";
    public const string PUSH_SERVICE_PATH = "/healthchecks/push";
    public const string HEALTH_CHECK_PATH_ANNOTATION = "HealthChecksPath";
    public const string HEALTH_CHECK_SCHEME_ANNOTATION = "HealthChecksScheme";
    public const string DEFAULT_PULL_POLICY = "Always";
    public const string DEFAULT_SERVICE_TYPE = "ClusterIP";
    public const string DEFAULT_UI_PATH = "/healthchecks";
    public const string DEFAULT_PORT = "80";
    public const string DEFAULT_SCHEME = "http";
    public const string DEFAULT_HEALTH_PATH = "health";
    public const string PUSH_SERVICE_AUTH_KEY = "key";
    public const string STYLES_PATH = "css";
    public const string STYLE_SHEET_NAME = "styles";

    internal class Deployment
    {
        internal class Operation
        {
            public const string ADD = "Add";
            public const string DELETE = "Delete";
            public const string PATCH = "Patch";
        }

        internal class Scope
        {
            public const string CLUSTER = "Cluster";
            public const string NAMESPACED = "Namespaced";
        }
    }
}
