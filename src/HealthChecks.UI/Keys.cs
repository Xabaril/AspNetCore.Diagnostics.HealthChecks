using System;

namespace HealthChecks.UI
{
    internal class Keys
    {
        internal const string HEALTHCHECKSUI_OLD_SECTION_SETTING_KEY = "HealthChecks-UI";
        internal const string HEALTHCHECKSUI_SECTION_SETTING_KEY = "HealthChecksUI";
        internal const string HEALTHCHECKS_DEFAULT_PATH = "hc";
        internal const string HEALTHCHECKS_DEFAULT_DISCOVERY_LABEL = "HealthChecks";
        internal const string HEALTHCHECKS_DEFAULT_DISCOVERY_PATH_ANNOTATION = "HealthChecksPath";
        internal const string HEALTHCHECKS_DEFAULT_DISCOVERY_PORT_ANNOTATION = "HealthChecksPort";
        internal const string HEALTHCHECKS_DEFAULT_DISCOVERY_SCHEME_ANNOTATION = "HealthChecksScheme";
        internal const string HEALTHCHECKSUI_KUBERNETES_DISCOVERY_SETTING_KEY = "HealthChecksUI:KubernetesDiscoveryService";        
        internal const string HEALTHCHECKSUI_MAIN_UI_RESOURCE = "index.html";
        internal const string HEALTHCHECKSUI_MAIN_UI_API_TARGET = "#apiPath#";
        internal const string HEALTHCHECKSUI_WEBHOOKS_API_TARGET = "#webhookPath#";
        internal const string HEALTHCHECKSUI_RESOURCES_TARGET = "#uiResourcePath#";
        internal const string HEALTHCHECKSUI_STYLESHEETS_TARGET = "#customstylesheets#";
        internal const string HEALTHCHECKSUI_ASIDEMENUEOPENED_TARGET = "#asideMenuOpened#";
        internal const string DEFAULT_RESPONSE_CONTENT_TYPE = "application/json";
        internal const string LIVENESS_BOOKMARK = "[[LIVENESS]]";
        internal const string FAILURE_BOOKMARK = "[[FAILURE]]";
        internal const string DESCRIPTIONS_BOOKMARK = "[[DESCRIPTIONS]]";
        internal const string HEALTH_CHECK_HTTP_CLIENT_NAME = "health-checks";
        internal const string HEALTH_CHECK_WEBHOOK_HTTP_CLIENT_NAME = "health-checks-webhooks";
        internal const string K8S_CLUSTER_SERVICE_HTTP_CLIENT_NAME = "k8s-cluster-service";
    }
}
