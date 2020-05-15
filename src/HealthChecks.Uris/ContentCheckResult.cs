namespace HealthChecks.Uris
{
    public abstract class ContentCheckResult
    {

    }

    public class ExpectedContentResult : ContentCheckResult
    {

    }

    public class UnexpectedContentResult : ContentCheckResult
    {
        public string Reason { get; }

        public UnexpectedContentResult(string reason)
        {
            Reason = reason;
        }
    }
}
