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
    /// <summary>
    ///   Sends memory and cpu usage of each process to CloudWatch.
    ///
    ///   A basic AWS Api account should be used for sending these metrics.
    /// </summary>
    public class ServicesMetricsJob : IJob
    {
        private readonly string InstanceId;
        private readonly string InstanceName;
        private readonly List<Dimension> MetricDatumDimensions;

        public ServicesMetricsJob()
        {
            InstanceName = Environment.MachineName;

            try
            {
                InstanceId = EC2InstanceMetadata.InstanceId;
            }
            catch
            {
                InstanceId = Environment.MachineName;
            }

            MetricDatumDimensions = new List<Dimension>
            {
                new Dimension
                {
                    Name = "InstanceID",
                    Value = InstanceId
                },
                new Dimension
                {
                    Name = "Instance Name",
                    Value = InstanceName
                }
            };
        }

        public void Execute()
        {
            // check only services from "company name" and a certain subsidiary
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
            var memoryUsage = 0.0;
            var cpuUsage = 0.0f;

            using (process)
            {
                var instance = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, true);
                memoryUsage = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2);
                cpuUsage = instance.NextValue();
            }

            client.PutMetricData(new PutMetricDataRequest
            {
                Namespace = $"MemoryCPUServicesUsage/{process.ProcessName}",
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
