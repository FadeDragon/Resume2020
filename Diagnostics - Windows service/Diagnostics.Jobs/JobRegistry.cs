using FluentScheduler;

namespace Diagnostics.Jobs
{
    /// <summary>
    ///   All Diagnostic check and service jobs are registered here
    /// </summary>
    public class JobRegistry : Registry
    {
        public JobRegistry()
        {
            // checks if services are running and restart/raise alerts if they are not
            Schedule<ServicesCheckJob>().WithName(typeof(ServicesCheckJob).Name).ToRunNow().AndEvery(30).Seconds();
            // send instance's memory and cpu metrics to CloudWatch
            Schedule<ServicesMetricsJob>().WithName(typeof(ServicesMetricsJob).Name).ToRunNow().AndEvery(5).Minutes();
        }
    }
}
