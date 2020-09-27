using System;
using System.Diagnostics.CodeAnalysis;
using Topshelf;

namespace Diagnostics.Service
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<DiagnosticsServices>(s =>
                {
                    s.ConstructUsing(name => new DiagnosticsServices());
                    s.WhenStarted(p => p.Start());
                    s.WhenStopped(p => p.Stop());
                });
                x.RunAsLocalSystem();
            });

            Environment.ExitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
        }

        /*
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddAWSProvider();

                    // When you need logging below set the minimum level. Otherwise the logging framework will default to Informational for external providers.
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .UseStartup<Startup>();
         */
    }
}
