using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudServiceTaskHost
{
    public class TasksRunner : ITasksRunner
    {
        private readonly IReadOnlyCollection<ITaskRunner> _runners;

        public TasksRunner(IReadOnlyCollection<ITaskRunner> runners)
        {
            if (runners == null) throw new ArgumentNullException("runners");
            _runners = runners;
        }

        public IReadOnlyCollection<ITaskRunner> TaskRunners
        {
            get { return _runners; }
        }

        public IEnumerable<Task> RunAsync()
        {
            return RunAsync(CancellationToken.None);
        }

        public IEnumerable<Task> RunAsync(CancellationToken cancellationToken)
        {
            return _runners.Select(runner => runner.RunAsync(cancellationToken));
        }
    }
}
