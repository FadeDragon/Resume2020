using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using FluentScheduler;
using Diagnostics.Jobs;

namespace Diagnostics.Service
{
    /// <summary>
    ///   The single service that TopShelf will run.
    ///
    ///   Uses Autofac for IoC dependency injection, and Fluent Scheduler to execute jobs at regular intervals
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DiagnosticsServices
    {
        private const int ExceptionLimitPerJob = 10;
        private static readonly ConcurrentDictionary<string, int> ExceptionTracker = new ConcurrentDictionary<string, int>();

        public void Start()
        {
            var builder = new ContainerBuilder();
            // register types and assemblies
            builder.RegisterModule<ContainerRegistrationsModule>();

            var container = builder.Build();

            JobManager.JobFactory = new AutofacJobFactory(container);

            JobManager.JobException += info =>
            {
                if (ExceptionTracker.ContainsKey(info.Name) && 
                    ExceptionTracker.TryGetValue(info.Name, out var counter) && counter > ExceptionLimitPerJob)
                {
                    // 10 exceptions raised for a job, let's stop it, and raise it to slack
                    JobManager.RemoveJob(info.Name);
                }
                else
                {
                    ExceptionTracker.AddOrUpdate(info.Name, x => 1, (key, oldValue) => oldValue + 1);
                }
            };

            // register diagnostic jobs
            JobManager.Initialize(new JobRegistry());
        }

        public void Stop()
        {
            JobManager.StopAndBlock();
        }
    }
}
