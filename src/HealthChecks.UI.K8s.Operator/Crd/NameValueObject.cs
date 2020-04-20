using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.UI.K8s.Operator.Crd
{
    public class NameValueObject
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
