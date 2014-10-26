using System.Threading;
using System.Threading.Tasks;

namespace CloudServiceTaskHost
{
    public interface ITaskRunner
    {
        Task RunAsync();
        Task RunAsync(CancellationToken cancellationToken);
    }
}