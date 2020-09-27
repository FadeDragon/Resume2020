using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.Runtime;
using Amazon.Util;
using FluentScheduler;

namespace Diagnostics.Jobs
{
    public class ServicesMetricsJob : IJob
    {
        private readonly string instanceId;
        private readonly string instanceName;
        private readonly List<Dimension> MetricDatumDimensions;

        public ServicesMetricsJob()
        {
            instanceName = Environment.MachineName;

            try
            {
                instanceId = EC2InstanceMetadata.InstanceId;
            }
            catch
            {
                instanceId = Environment.MachineName;
            }

            MetricDatumDimensions = new List<Dimension>
            {
                new Dimension
                {
                    Name = "InstanceID",
                    Value = instanceId
                },
                new Dimension
                {
                    Name = "Instance Name",
                    Value = instanceName
                }
            };
        }

        public void Execute()
        {
            var processes = Process.GetProcesses()
                                   .Where(x => x.ProcessName.ToLower().Contains("company product") ||
                                               x.ProcessName.ToLower().Contains("subsidiary project 1"))
                                   .ToList();

            var awsCreds = new BasicAWSCredentials("", "");
            var client = new AmazonCloudWatchClient(awsCreds, RegionEndpoint.USWest2);

            foreach (var process in processes)
            {
                PutPerformanceMetricDataForProcess(client, process);
            }
        }

        private void PutPerformanceMetricDataForProcess(AmazonCloudWatchClient client, Process process)
        {
            double memoryUsage = 0.0;
            float cpuUsage = 0.0f;

            using (process)
            {
                var instance = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, true);
                memoryUsage = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2);
                cpuUsage = instance.NextValue();
            }

            client.PutMetricData(new PutMetricDataRequest
            {
                Namespace = $"CustomAggregateInstanceUsage/{process.ProcessName}",
                MetricData = new List<MetricDatum>
                {
                    new MetricDatum
                    {
                        MetricName = "Memory Usage",
                        Unit = StandardUnit.Megabits,
                        Value = memoryUsage,
                        Dimensions = MetricDatumDimensions
                    },
                    new MetricDatum
                    {
                        MetricName = "CPU Usage",
                        Unit = StandardUnit.Percent,
                        Value = cpuUsage,
                        Dimensions = MetricDatumDimensions
                    }
                }
            });
        }
    }
}
