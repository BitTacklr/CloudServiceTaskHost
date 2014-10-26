using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudServiceTaskHost
{
    public class FaultLoggingTaskRunner : TaskRunner
    {
        private readonly ITaskRunner _next;
        private readonly Action<Exception> _logger;

        public FaultLoggingTaskRunner(ITaskRunner next, Action<Exception> logger)
        {
            if (next == null) throw new ArgumentNullException("next");
            if (logger == null) throw new ArgumentNullException("logger");
            _next = next;
            _logger = logger;
        }

        public override Task RunAsync(CancellationToken cancellationToken)
        {
            return _next.
                RunAsync(cancellationToken).
                ContinueWith(task =>
                {
                    if (!task.IsFaulted || task.Exception == null) return;

                    _logger(task.Exception);

                    throw task.Exception;
                }, cancellationToken);
        }
    }
}