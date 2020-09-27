using FluentScheduler;

namespace Diagnostics.Jobs
{
    public class JobRegistry : Registry
    {
        public JobRegistry()
        {
            Schedule<ServicesCheckJob>().WithName(typeof(ServicesCheckJob).Name).ToRunNow().AndEvery(30).Seconds();
            Schedule<ServicesMetricsJob>().WithName(typeof(ServicesMetricsJob).Name).ToRunNow().AndEvery(5).Minutes();
        }
    }
}
