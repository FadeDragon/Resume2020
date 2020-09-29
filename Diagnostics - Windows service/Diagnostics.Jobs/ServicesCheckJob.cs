using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using FluentScheduler;
using Diagnostics.Jobs.SlackMessageSender;

namespace Diagnostics.Jobs
{
    /// <summary>
    ///   Checks that services with "company name" in the service name are running.
    ///
    ///   If a service is not Running, this job will attempt to restart it every 5 minutes.
    ///
    ///   After 10 minutes, a message will be sent to a slack channel defined in the web hook.
    /// </summary>
    public class ServicesCheckJob : IJob
    {
        private static readonly ConcurrentDictionary<string, ServiceState> State = new ConcurrentDictionary<string, ServiceState>();
        private readonly ISlackMessageSender SlackSender;
        private static readonly string EnvironmentName = ConfigurationManager.AppSettings["Environment"];

        // all times in minutes
        private static readonly int TimeBeforeNotification = 10;
        private static readonly int TimeBeforeRestartAttempt = 5;

        public ServicesCheckJob(ISlackMessageSender slackSender)
        {
            SlackSender = slackSender;
        }

        public void Execute()
        {
            // check only services from "company name", and skip checking all diagnostics related services
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
                
                // reaching this section means the service is not running
                serviceState.MarkAsDown();

                switch (serviceState.IsDownForTooLong())
                {
                    case ServiceStateReaction.DownButAttemptToRestart:
                        {
                            // attempt a restart and increment the restart count
                            TryRestart(service, serviceState);
                            break;
                        }
                    case ServiceStateReaction.RestartFailedTooManyTimesRaiseAlert:
                        {
                            if (!serviceState.NotificationHasBeenSent)
                            {
                                // send alert notification and set NotificationHasBeenSent=true
                                SendAlert(serviceState);
                            }
                            break;
                        }
                    // case ServiceStateReaction.AcceptableDuration:
                    // Down but within acceptable duration
                    default:
                        {
                            continue;
                        }
                }
            }
        }

        static void TryRestart(ServiceController service, ServiceState serviceState)
        {
            try
            {
                service.Start();
            }
            catch
            {
                // ignore - will try again soon and alerts will eventually be raised.
            }
            serviceState.RestartAttempted();
        }

        private void SendAlert(ServiceState serviceState)
        {
            // send notification into slack channel to alert team
            var message = $@"
[Diagnostics - {EnvironmentName}] Windows Service {serviceState.DisplayName} is down!

Detected to be down at {serviceState.FirstDown:D} UTC and has failed to be restarted {serviceState.RestartCount} time{(serviceState.RestartCount == 1 ? "" : "s")}.
";

            SlackSender.Send(message);
            
            serviceState.NotificationRaised();
        }

        private enum ServiceStateReaction
        {
            DownButAttemptToRestart,
            RestartFailedTooManyTimesRaiseAlert,
            AcceptableDuration
        }

        private class ServiceState
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
