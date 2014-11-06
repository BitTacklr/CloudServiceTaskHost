using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CloudServiceTaskHost
{
    public class ProcessTaskRunner : TaskRunner
    {
        private const string WorkerRoleShutdownFileEnvironmentVariableName = "WORKERJOBS_SHUTDOWN_FILE";

        private readonly ProcessStartInfo _startInfo;
        private readonly TimeSpan _waitForExitTimeout;

        public ProcessTaskRunner(ProcessStartInfo startInfo, TimeSpan waitForExitTimeout)
        {
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            _startInfo = startInfo;
            _waitForExitTimeout = waitForExitTimeout;
        }

        public override Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.
                StartNew(
                    () =>
                    {
                        using (var shutdownFile = ShutdownFile.CreateRandom(
                            new DirectoryInfo(
                                Path.GetDirectoryName(_startInfo.FileName))))
                        {
                            _startInfo.EnvironmentVariables[WorkerRoleShutdownFileEnvironmentVariableName] = shutdownFile.FullName;
                            using (var process = Process.Start(_startInfo))
                            {
                                cancellationToken.WaitHandle.WaitOne();
                                shutdownFile.Notify();
                                if (!process.WaitForExit(Convert.ToInt32(_waitForExitTimeout.TotalMilliseconds)))
                                {
                                    process.Kill();
                                }
                            }
                        }
                    },
                    cancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
        }
    }
}