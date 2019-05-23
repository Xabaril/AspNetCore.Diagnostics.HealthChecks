using System;
using Docker.DotNet.Models;

namespace HealthChecks.UI.Core.Discovery.Docker.Extensions
{
    internal static class DockerDiscoveryExtensions
    {
        internal static bool TryGetLabel<T>(this ContainerListResponse container, string label, out T value)
        {
            if (!container.Labels.TryGetValue(label, out var val))
            {
                value = default;
                return false;
            }

            value = (T)Convert.ChangeType(val, typeof(T));
            return true;
        }

        internal static T GetLabel<T>(this ContainerListResponse container, string label, T @default = default)
        {
            if (!TryGetLabel(container, label, out T val))
                return @default;

            return val;
        }
    }
}
