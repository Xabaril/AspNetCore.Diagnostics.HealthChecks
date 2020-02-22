using HealthChecks.UI.Core.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HealthChecks.UI.Core
{
    public class HealthCheckConfigurationEqualityComparer : IEqualityComparer<HealthCheckConfiguration>
    {
        public bool Equals([AllowNull] HealthCheckConfiguration x, [AllowNull] HealthCheckConfiguration y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode([DisallowNull] HealthCheckConfiguration obj)
        {
            return obj.GetHashCode();
        }
    }
}
