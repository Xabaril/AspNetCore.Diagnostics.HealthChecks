using System.Collections.Generic;

namespace HealthChecks.UI.Core
{
    interface IUIResourcesReader
    {
        IEnumerable<UIResource> UIResources { get; }
    }
}