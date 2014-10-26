using System.Threading;
using System.Threading.Tasks;

namespace CloudServiceTaskHost
{
    public abstract class TaskRunner : ITaskRunner
    {
        public Task RunAsync()
        {
            return RunAsync(CancellationToken.None);
        }

        public abstract Task RunAsync(CancellationToken cancellationToken);
    }
}