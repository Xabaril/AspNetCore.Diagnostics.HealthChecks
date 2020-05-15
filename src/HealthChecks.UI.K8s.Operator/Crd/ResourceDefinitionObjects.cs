namespace HealthChecks.UI.K8s.Operator.Crd
{
    public class NameValueObject
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class WebHookObject
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Payload { get; set; }
        public string RestoredPayload { get; set; }
    }
}
