namespace HealthChecks.Network.Extensions
{
    public static class TaskExtensions
    {
        public static async Task WithCancellationTokenAsync(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            cancellationToken.Register(() => tcs.SetResult(true));

            if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
            {
                throw new OperationCanceledException("The operation has timed out", cancellationToken);
            }

            await task.ConfigureAwait(false);
        }

        public static async Task<T> WithCancellationTokenAsync<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();

            cancellationToken.Register(() => tcs.SetResult(default!));

            if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
            {
                throw new OperationCanceledException("The operation has timed out", cancellationToken);
            }
            else
            {
                return await task.ConfigureAwait(false);
            }
        }
    }
}
