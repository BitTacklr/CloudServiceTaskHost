using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudServiceTaskHost
{
    public interface ITasksRunner
    {
        IEnumerable<Task> RunAsync();
        IEnumerable<Task> RunAsync(CancellationToken cancellationToken);
    }
}