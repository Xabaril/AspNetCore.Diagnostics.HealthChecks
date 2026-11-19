using HealthChecks.UI.K8s.Operator.Diagnostics;
using k8s.Models;

namespace HealthChecks.UI.K8s.Operator;

public static class ContainerExtensions
{
    internal static void MapCustomUIPaths(this V1Container container, HealthCheckResource resource, OperatorDiagnostics diagnostics)
    {

        var uiPath = resource.Spec.UiPath ?? Constants.DEFAULT_UI_PATH;
        container.Env.Add(new V1EnvVar { Name = "ui_path", Value = uiPath });
        diagnostics.UiPathConfigured(nameof(resource.Spec.UiPath), uiPath);

        if (!string.IsNullOrEmpty(resource.Spec.UiApiPath))
        {
            container.Env.Add(new V1EnvVar { Name = "ui_api_path", Value = resource.Spec.UiApiPath });
            diagnostics.UiPathConfigured(nameof(resource.Spec.UiApiPath), resource.Spec.UiApiPath);
        }

        if (!string.IsNullOrEmpty(resource.Spec.UiResourcesPath))
        {
            container.Env.Add(new V1EnvVar { Name = "ui_resources_path", Value = resource.Spec.UiResourcesPath });
            diagnostics.UiPathConfigured(nameof(resource.Spec.UiResourcesPath), resource.Spec.UiResourcesPath);
        }

        if (!string.IsNullOrEmpty(resource.Spec.UiWebhooksPath))
        {
            container.Env.Add(new V1EnvVar { Name = "ui_webhooks_path", Value = resource.Spec.UiWebhooksPath });
            diagnostics.UiPathConfigured(nameof(resource.Spec.UiWebhooksPath), resource.Spec.UiWebhooksPath);
        }

        if (resource.Spec.UiNoRelativePaths.HasValue)
        {
            var noRelativePaths = resource.Spec.UiNoRelativePaths.Value.ToString();
            container.Env.Add(new V1EnvVar { Name = "ui_no_relative_paths", Value = noRelativePaths });
            diagnostics.UiPathConfigured(nameof(resource.Spec.UiNoRelativePaths), noRelativePaths);
        }
    }
}
