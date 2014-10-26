using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudServiceTaskHost
{
    public class RetryingTaskRunner : TaskRunner
    {
        private readonly ITaskRunner _next;
        private readonly int _retryCount;
        private readonly TimeSpan _timeBetweenRetries;

        public RetryingTaskRunner(ITaskRunner next, int retryCount, TimeSpan timeBetweenRetries)
        {
            if (next == null) 
                throw new ArgumentNullException("next");
            if (retryCount < 0) 
                throw new ArgumentOutOfRangeException(
                    "retryCount", 
                    retryCount, 
                    "The retry count must be greater than or equal to 0.");
            if (timeBetweenRetries < TimeSpan.Zero) 
                throw new ArgumentOutOfRangeException(
                    "timeBetweenRetries", 
                    timeBetweenRetries, 
                    "The time between retries must be greater than or equal to 0.");
            _next = next;
            _retryCount = retryCount;
            _timeBetweenRetries = timeBetweenRetries;
        }

        public override Task RunAsync(CancellationToken cancellationToken)
        {
            return _next.
                RunAsync(cancellationToken).
                ContinueWith(
                    task => RetryAsync(task, cancellationToken, _retryCount, _timeBetweenRetries), 
                    cancellationToken).
                Unwrap();
        }

        private async Task RetryAsync(Task task, CancellationToken cancellationToken, int restCount, TimeSpan timeBetweenRetries)
        {
            if (!task.IsFaulted || task.Exception == null) return;
            if (restCount == 0) throw task.Exception;
            
            await Task.Delay(timeBetweenRetries, cancellationToken);

            await _next.
                RunAsync(cancellationToken).
                ContinueWith(
                    _ => RetryAsync(_, cancellationToken, restCount - 1, timeBetweenRetries),
                    cancellationToken);
        }
    }
}
