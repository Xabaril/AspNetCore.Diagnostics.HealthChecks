using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core
{
    public class HealthCheckDbOptions
    {
        public string Schema { get; private set; } = string.Empty;
        public bool DatabaseMigrationsEnabled { get; internal set; }
        public HealthCheckDbOptions UseSchema(string schema)
        {
            Schema = schema;
            return this;
        }

        public HealthCheckDbOptions DisableMigrations(bool disable)
        {
            DatabaseMigrationsEnabled = disable;
            return this;
        }

    }
}
