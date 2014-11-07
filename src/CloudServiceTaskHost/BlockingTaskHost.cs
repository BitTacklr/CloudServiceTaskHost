using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudServiceTaskHost
{
    public class BlockingTaskHost
    {
        private readonly ITasksRunner _runner;
        private readonly TimeSpan _timeout;

        public BlockingTaskHost(ITasksRunner runner, TimeSpan timeout)
        {
            if (runner == null) throw new ArgumentNullException("runner");
            if (timeout <TimeSpan.Zero) throw new ArgumentOutOfRangeException("timeout", timeout, "The timeout must greater than or equal to zero.");
            _runner = runner;
            _timeout = timeout;
        }

        public void Run(CancellationToken token)
        {
            var tasks = _runner.
                RunAsync(token).
                ToArray();
            token.WaitHandle.WaitOne();
            Task.WaitAll(tasks, _timeout);
        }
    }
}
