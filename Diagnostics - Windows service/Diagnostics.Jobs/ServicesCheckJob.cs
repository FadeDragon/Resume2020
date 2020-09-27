using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using FluentScheduler;
using Diagnostics.Jobs.SlackMessageSender;

namespace Diagnostics.Jobs
{
    public class ServicesCheckJob : IJob
    {
        private static readonly ConcurrentDictionary<string, ServiceState> State = new ConcurrentDictionary<string, ServiceState>();
        private readonly ISlackMessageSender slackSender;
        private static readonly string EnvironmentName = ConfigurationManager.AppSettings["Environment"];

        private static readonly int TimeBeforeNotification = 10; // 10 minutes
        private static readonly int TimeBeforeRestartAttempt = 5; // 5 minutes

        public ServicesCheckJob(ISlackMessageSender slackSender)
        {
            this.slackSender = slackSender;
        }

        public void Execute()
        {
            var services = ServiceController.GetServices()
                                            .Where(x => x.ServiceName.ToLower().Contains("company name"))
                                            .Where(x => !x.ServiceName.ToLower().Contains("diagnostics"))
                                            .ToList();

            foreach (var service in services)
            {
                var serviceState = State.GetOrAdd(service.ServiceName, x => new ServiceState(x));

                if (service.Status == ServiceControllerStatus.Running)
                {
                    serviceState.Clear();

                    continue;
                }

                serviceState.MarkAsDown();

                switch (serviceState.IsDownForTooLong())
                {
                    case ServiceStateReaction.DownButAttemptToRestart:
                        {
                            TryRestart(service);
                            serviceState.RestartAttempted();
                            break;
                        }
                    case ServiceStateReaction.RestartFailedTooManyTimesRaiseAlert:
                        {
                            if (!serviceState.NotificationHasBeenSent)
                            {
                                SendAlert(serviceState);
                                serviceState.NotificationRaised();
                            }
                            break;
                        }
                    default:
                        {
                            // Down but not long enough, still acceptable
                            continue;
                        }
                }
            }
        }

        void TryRestart(ServiceController service)
        {
            try
            {
                service.Start();
            }
            catch
            {
                // Ignore - next iteration will try again and after 10 minutes notify us.
            }
        }

        void SendAlert(ServiceState serviceState)
        {
            // Send notification slack channel to alert team
            var message = $@"
[Diagnostics - {EnvironmentName}] Windows Service {serviceState.DisplayName} is down!

It was first noticed to be down at {serviceState.FirstDown:D} UTC and has failed to be restarted {serviceState.RestartCount} time{(serviceState.RestartCount == 1 ? "" : "s")}.
";

            slackSender.Send(message);
        }

        public enum ServiceStateReaction
        {
            DownButAttemptToRestart,
            RestartFailedTooManyTimesRaiseAlert,
            AcceptableDuration
        }

        public class ServiceState
        {
            public ServiceState(string displayName)
            {
                DisplayName = displayName;
            }

            public string DisplayName { get; }

            public DateTime FirstDown { get; set; } = DateTime.MinValue;

            public bool NotificationHasBeenSent { get; private set; }

            public int RestartCount { get; private set; }

            public void Clear()
            {
                FirstDown = DateTime.MinValue;
            }

            public void MarkAsDown()
            {
                if (FirstDown == DateTime.MinValue)
                {
                    FirstDown = DateTime.UtcNow;
                }
            }

            public void RestartAttempted()
            {
                RestartCount += 1;
            }

            public ServiceStateReaction IsDownForTooLong()
            {
                if (FirstDown.AddMinutes(TimeBeforeRestartAttempt) > DateTime.UtcNow && FirstDown.AddMinutes(TimeBeforeNotification) > DateTime.UtcNow)
                {
                    return ServiceStateReaction.AcceptableDuration;
                }

                if (FirstDown.AddMinutes(TimeBeforeRestartAttempt) < DateTime.UtcNow && FirstDown.AddMinutes(TimeBeforeNotification) > DateTime.UtcNow)
                {
                    return ServiceStateReaction.DownButAttemptToRestart;
                }

                return ServiceStateReaction.RestartFailedTooManyTimesRaiseAlert;
            }

            public void NotificationRaised()
            {
                NotificationHasBeenSent = true;
            }
        }
    }
}
