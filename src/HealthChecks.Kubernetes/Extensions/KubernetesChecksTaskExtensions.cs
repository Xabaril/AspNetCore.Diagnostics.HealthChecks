namespace System.Threading.Tasks
{
    public static class KubernetesChecksTaskExtensions
    {
        public static Task<(bool result, string name)[]> PreserveMultipleExceptions(this Task<(bool, string)[]> task)
        {
            var tcs = new TaskCompletionSource<(bool, string)[]>();
            task.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.Canceled:
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.RanToCompletion:
                        tcs.SetResult(t.Result);
                        break;
                    case TaskStatus.Faulted:
                        tcs.SetException(t.Exception);
                        break;
                }
            }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }
    }
}
