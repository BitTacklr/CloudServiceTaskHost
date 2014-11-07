using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CloudServiceTaskHost
{
    [TestFixture]
    public class BlockingTaskHostTests
    {
        [Test]
        public void TasksRunnerCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BlockingTaskHost(null, TimeSpan.Zero));
        }

        [Test]
        public void TimeoutCanNotBeNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BlockingTaskHost(
                    new TasksRunner(new List<ITaskRunner>()),
                    TimeSpan.MinValue));
        }

        [TestCase(0L)]
        [TestCase(Int64.MaxValue)]
        public void TimeoutCanBeZeroOrPositive(long ticks)
        {
            Assert.DoesNotThrow(() => 
                new BlockingTaskHost(
                    new TasksRunner(new List<ITaskRunner>()), 
                    TimeSpan.FromTicks(ticks)));  
        }

        [Test]
        public void RunHasExpectedBehavior()
        {
            var runner = new StubbedTaskRunner();
            var tasksRunner = new TasksRunner(new List<ITaskRunner> {runner});
            var sut = new BlockingTaskHost(tasksRunner, TimeSpan.Zero);
            var tokenSource = new CancellationTokenSource();
            using (new Timer(
                state => tokenSource.Cancel(),
                null,
                TimeSpan.FromSeconds(0.5),
                TimeSpan.FromTicks(-1)))
            {
                sut.Run(tokenSource.Token);
            }
            Assert.That(runner.CallCount, Is.EqualTo(1));
        }

        [Test, Repeat(2)]
        public void RunCompletesAfterTimeoutEvenIfTaskNeverEnds()
        {
            var runner = new NeverEndingTaskRunner();
            var tasksRunner = new TasksRunner(new List<ITaskRunner> { runner });
            var sut = new BlockingTaskHost(tasksRunner, TimeSpan.FromSeconds(0.5));
            var tokenSource = new CancellationTokenSource();
            using (new Timer(
                state => tokenSource.Cancel(),
                null,
                TimeSpan.FromSeconds(0.5),
                TimeSpan.FromTicks(-1)))
            {
                sut.Run(tokenSource.Token);
            }
            Assert.That(runner.CallCount, Is.EqualTo(1));
        }

        class StubbedTaskRunner : TaskRunner
        {
            private readonly Task _task;

            public StubbedTaskRunner()
            {
                _task = Task.FromResult(false);
                CallCount = 0;
            }

            public Task Task
            {
                get { return _task; }
            }

            public int CallCount { get; private set; }

            public override Task RunAsync(CancellationToken cancellationToken)
            {
                CallCount++;
                return Task;
            }
        }

        class NeverEndingTaskRunner : TaskRunner
        {
            private readonly Task _task;

            public NeverEndingTaskRunner()
            {
                _task = new TaskCompletionSource<bool>().Task;
                CallCount = 0;
            }

            public Task Task
            {
                get { return _task; }
            }

            public int CallCount { get; private set; }

            public override Task RunAsync(CancellationToken cancellationToken)
            {
                CallCount++;
                return Task;
            }
        }
    }
}
