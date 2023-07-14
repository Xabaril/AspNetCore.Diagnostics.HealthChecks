namespace HealthChecks.Network.Extensions;

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

    internal static bool ContainsArray(this byte[] source, ReadOnlySpan<byte> segment)
    {
        if (segment.Length == 0)
            return true;

        for (int i = 0; i < source.Length - segment.Length + 1; ++i)
        {
            for (int j = 0; j < segment.Length; ++j)
            {
                if (source[i + j] != segment[j])
                    break;
                if (j == segment.Length - 1)
                    return true;
            }
        }

        return false;
    }
}
