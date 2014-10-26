using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CloudServiceTaskHost
{
    [TestFixture]
    public class RetryingTaskRunnerTests
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

        [TestCase(-1)]
        [TestCase(Int32.MinValue)]
        public void RetryCountCanNotBeLessThan0(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SutFactoryWithRetryCount(value));
        }

        [TestCase(-1L)]
        [TestCase(Int64.MinValue)]
        public void TimeBetweenRetriesCanNotBeLessThanZero(long value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SutFactoryWithTimeBetweenRetries(TimeSpan.FromTicks(value)));
        }

        [Test]
        public async Task RunAsyncWithoutTokenRetriesWhenNextRunnerFails()
        {
            var next = new FaultyTaskRunner(new Exception("Message"), 1);
            var sut = SutFactory(next, 1, TimeSpan.Zero);
            await sut.RunAsync();
            Assert.That(next.RunCount, Is.EqualTo(2));
        }

        [Test]
        public void RunAsyncWithoutTokenThrowsWhenRetryCountReached()
        {
            var next = new FaultyTaskRunner(new Exception("Message"), 1);
            var sut = SutFactory(next, 0, TimeSpan.Zero);
            Assert.Throws<AggregateException>(async () => await sut.RunAsync());
            Assert.That(next.RunCount, Is.EqualTo(1));
        }

        [Test]
        public async Task RunAsyncWithTokenRetriesWhenNextRunnerFails()
        {
            var next = new FaultyTaskRunner(new Exception("Message"), 1);
            var sut = SutFactory(next, 2, TimeSpan.Zero);
            await sut.RunAsync(CancellationToken.None);
            Assert.That(next.RunCount, Is.EqualTo(2));
        }

        [Test]
        public void RunAsyncWithTokenThrowsWhenRetryCountReached()
        {
            var next = new FaultyTaskRunner(new Exception("Message"), 1);
            var sut = SutFactory(next, 0, TimeSpan.Zero);
            Assert.Throws<AggregateException>(async () => await sut.RunAsync(CancellationToken.None));
            Assert.That(next.RunCount, Is.EqualTo(1));
        }

        [Test]
        public async Task RunAsyncWithCancelledTokenDoesNotRetryWhenNextRunnerFails()
        {
            var next = new FaultyTaskRunner(new Exception("Message"), 1);
            var sut = SutFactory(next, 1, TimeSpan.Zero);
            var source = new CancellationTokenSource();
            source.Cancel();
            Assert.Throws<TaskCanceledException>(async () => await sut.RunAsync(source.Token));
            Assert.That(next.RunCount, Is.EqualTo(1));
        }

        [Test]
        public void RunAsyncWithCancelledTokenThrowsWhenRetryCountReached()
        {
            var next = new FaultyTaskRunner(new Exception("Message"), 1);
            var sut = SutFactory(next, 0, TimeSpan.Zero);
            var source = new CancellationTokenSource();
            source.Cancel();
            Assert.Throws<TaskCanceledException>(async () => await sut.RunAsync(source.Token));
            Assert.That(next.RunCount, Is.EqualTo(1));
        }

        private static RetryingTaskRunner SutFactoryWithTimeBetweenRetries(TimeSpan value)
        {
            return SutFactory(
                new StubbedTaskRunner(),
                0,
                value);
        }

        private static RetryingTaskRunner SutFactoryWithRetryCount(int value)
        {
            return SutFactory(
                new StubbedTaskRunner(),
                value, 
                TimeSpan.Zero);
        }

        private static RetryingTaskRunner SutFactoryWithNext(ITaskRunner next)
        {
            return SutFactory(
                next,
                0,
                TimeSpan.Zero);
        }

        private static RetryingTaskRunner SutFactory()
        {
            return SutFactory(
                new StubbedTaskRunner(),
                0,
                TimeSpan.Zero);
        }

        private static RetryingTaskRunner SutFactory(ITaskRunner next, int retryCount, TimeSpan timeBetweenRetries)
        {
            return new RetryingTaskRunner(
                next,
                retryCount,
                timeBetweenRetries);
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
            private int _failCount;
            private readonly Task _faultyTask;
            private readonly Task _flawlessTask;
            private int _runCount;

            public FaultyTaskRunner(Exception exception, int failCount)
            {
                var source = new TaskCompletionSource<bool>();
                source.SetException(exception);
                _faultyTask = source.Task;
                _flawlessTask = Task.FromResult(false);
                _failCount = failCount;
                _runCount = 0;
            }

            public Task FlawlessTask
            {
                get { return _flawlessTask; }
            }

            public Task FaultyTask
            {
                get { return _faultyTask; }
            }

            public int RunCount
            {
                get { return _runCount; }
            }

            public override Task RunAsync(CancellationToken cancellationToken)
            {
                _runCount += 1;
                return _failCount-- == 0 ? FlawlessTask : FaultyTask;
            }
        }
    }
}
