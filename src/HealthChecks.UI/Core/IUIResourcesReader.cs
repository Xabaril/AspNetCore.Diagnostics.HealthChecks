namespace HealthChecks.UI.Core;

internal interface IUIResourcesReader
{
    IEnumerable<UIResource> UIResources { get; }
}
