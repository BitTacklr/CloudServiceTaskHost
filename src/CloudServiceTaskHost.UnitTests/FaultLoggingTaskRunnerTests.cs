using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CloudServiceTaskHost
{
    [TestFixture]
    public class FaultLoggingTaskRunnerTests
    {
        [Test]
        public void IsTaskRunner()
        {
            Assert.That(SutFactory(), Is.InstanceOf<ITaskRunner>());
        }

        [Test]
        public void NextCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => SutFactoryWithNext(null));
        }

        [Test]
        public void LoggerCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => SutFactoryWithLogger(null));
        }

        [Test]
        public void RunAsyncWithoutTokenWhenNextSucceedsHasExpectedResult()
        {
            var callCount = 0;
            var logger = new Action<Exception>(_ => callCount++);
            var next = new FlawlessTaskRunner();
            var sut = SutFactory(next, logger);
            sut.RunAsync().Wait();
            Assert.That(callCount, Is.EqualTo(0));
        }

        [Test]
        public void RunAsyncWithoutTokenWhenNextFailsHasExpectedResult()
        {
            var callCount = 0;
            var exception = new Exception("Message");
            var logger = new Action<Exception>(_ =>
            {
                if (ReferenceEquals(((AggregateException)_).Flatten().InnerException, exception)) callCount++;
            });
            var next = new FaultyTaskRunner(exception);
            var sut = SutFactory(next, logger);
            Assert.Throws<AggregateException>(async () => await sut.RunAsync());
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void RunAsyncWithTokenWhenNextSucceedsHasExpectedResult()
        {
            var callCount = 0;
            var logger = new Action<Exception>(_ => callCount++);
            var next = new FlawlessTaskRunner();
            var sut = SutFactory(next, logger);
            var source = new CancellationTokenSource();
            sut.RunAsync(source.Token).Wait(source.Token);
            Assert.That(callCount, Is.EqualTo(0));
        }

        [Test]
        public void RunAsyncWithCancelledTokenWhenNextSucceedsHasExpectedResult()
        {
            var callCount = 0;
            var logger = new Action<Exception>(_ => callCount++);
            var next = new FlawlessTaskRunner();
            var sut = SutFactory(next, logger);
            var source = new CancellationTokenSource();
            source.Cancel();
            Assert.Throws<OperationCanceledException>(() => sut.RunAsync(source.Token).Wait(source.Token));
            Assert.That(callCount, Is.EqualTo(0));
        }

        [Test]
        public void RunAsyncWithTokenWhenNextFailsHasExpectedResult()
        {
            var callCount = 0;
            var exception = new Exception("Message");
            var logger = new Action<Exception>(_ =>
            {
                if (ReferenceEquals(((AggregateException)_).Flatten().InnerException, exception)) callCount++;
            });
            var next = new FaultyTaskRunner(exception);
            var sut = SutFactory(next, logger);
            var source = new CancellationTokenSource();
            Assert.Throws<AggregateException>(async () => await sut.RunAsync(source.Token));
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void RunAsyncWithCancelledTokenWhenNextFailsHasExpectedResult()
        {
            var callCount = 0;
            var logger = new Action<Exception>(_ => callCount++);
            var next = new FaultyTaskRunner(new Exception("Message"));
            var sut = SutFactory(next, logger);
            var source = new CancellationTokenSource();
            source.Cancel();
            Assert.Throws<OperationCanceledException>(() => sut.RunAsync(source.Token).Wait(source.Token));
            Assert.That(callCount, Is.EqualTo(0));
        }

        private static FaultLoggingTaskRunner SutFactoryWithLogger(Action<Exception> logger)
        {
            return SutFactory(
                new StubbedTaskRunner(),
                logger);
        }

        private static FaultLoggingTaskRunner SutFactoryWithNext(ITaskRunner next)
        {
            return SutFactory(
                next,
                _ => {});
        }

        private static FaultLoggingTaskRunner SutFactory()
        {
            return SutFactory(
                new StubbedTaskRunner(), 
                _ => {});
        }

        private static FaultLoggingTaskRunner SutFactory(ITaskRunner next, Action<Exception> logger)
        {
            return new FaultLoggingTaskRunner(
                next,
                logger);
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

        class FaultyTaskRunner : TaskRunner
        {
            private readonly Task _task;

            public FaultyTaskRunner(Exception exception)
            {
                var source = new TaskCompletionSource<bool>();
                source.SetException(exception);
                _task = source.Task;
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

        class FlawlessTaskRunner : TaskRunner
        {
            private readonly Task _task;

            public FlawlessTaskRunner()
            {
                var source = new TaskCompletionSource<bool>();
                source.SetResult(true);
                _task = source.Task;
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
