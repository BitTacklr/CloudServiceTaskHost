using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CloudServiceTaskHost
{
    [TestFixture]
    public class TasksRunnerTests
    {
        private TasksRunner _sut;
        private ITaskRunner[] _runners;
        private StubbedTaskRunner _runner1;
        private StubbedTaskRunner _runner2;

        [SetUp]
        public void SetUp()
        {
            _runner1 = new StubbedTaskRunner();
            _runner2 = new StubbedTaskRunner();
            _runners = new ITaskRunner[]
            {
                _runner1,
                _runner2
            };
            _sut = new TasksRunner(
                new ReadOnlyCollection<ITaskRunner>(
                    _runners));
        }

        [Test]
        public void IsMultiTaskRoleEntryPoint()
        {
            Assert.That(_sut, Is.InstanceOf<ITasksRunner>());
        }

        [Test]
        public void RunnersReturnsExpectedResult()
        {
            Assert.That(_sut.TaskRunners, Is.EquivalentTo(_runners));
        }

        [Test]
        public void RunAsyncWithoutTokenReturnsExpectedResult()
        {
            var result = _sut.RunAsync();
            Assert.That(result, Is.EquivalentTo(new[] { _runner1.Task, _runner2.Task }));
        }

        [Test]
        public void RunAsyncWithTokenReturnsExpectedResult()
        {
            var result = _sut.RunAsync(CancellationToken.None);
            Assert.That(result, Is.EquivalentTo(new[] { _runner1.Task, _runner2.Task }));
        }

        class StubbedTaskRunner : ITaskRunner
        {
            private readonly Task _task;

            public StubbedTaskRunner()
            {
                _task = Task.FromResult(false);
            }

            public Task Task
            {
                get { return _task; }
            }

            public Task RunAsync()
            {
                return RunAsync(CancellationToken.None);
            }

            public Task RunAsync(CancellationToken cancellationToken)
            {
                return Task;
            }
        }
    }
}
