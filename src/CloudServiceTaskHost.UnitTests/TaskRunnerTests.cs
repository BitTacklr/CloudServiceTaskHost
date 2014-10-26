using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CloudServiceTaskHost
{
    [TestFixture]
    public class TaskRunnerTests
    {
        [Test]
        public void IsTaskRunner()
        {
            var sut = new StubbedTaskRunner();
            Assert.That(sut, Is.InstanceOf<ITaskRunner>());
        }

        [Test]
        public void RunAsyncWithoutTokenReturnsExpectedResult()
        {
            var sut = new StubbedTaskRunner();
            var result = sut.RunAsync();
            Assert.That(result, Is.EqualTo(sut.Task));
        }

        [Test]
        public void RunAsyncWithTokenReturnsExpectedResult()
        {
            var source = new CancellationTokenSource();
            var sut = new StubbedTaskRunner();
            var result = sut.RunAsync(source.Token);
            Assert.That(result, Is.EqualTo(sut.Task));
        }

        class StubbedTaskRunner : TaskRunner
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

            public override Task RunAsync(CancellationToken cancellationToken)
            {
                return Task;
            }
        }
    }
}