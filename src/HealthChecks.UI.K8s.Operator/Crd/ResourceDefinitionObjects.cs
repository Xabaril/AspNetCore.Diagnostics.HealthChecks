namespace HealthChecks.UI.K8s.Operator.Crd
{
    public class NameValueObject
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TolerationObject
    {
        public string Key { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string Effect { get; set; }
        public long? Seconds { get; set; }
    }

    public class WebHookObject
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Payload { get; set; }
        public string RestoredPayload { get; set; }
    }
}
