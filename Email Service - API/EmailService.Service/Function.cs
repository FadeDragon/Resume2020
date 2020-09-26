using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleEmail;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using Mindscape.Raygun4Net;
using EmailService.Core;
using EmailService.Core.Data;
using EmailService.Core.Queue;
using EmailService.Service.Services;
//using EmailService.Service.Templating;
//using SendGrid;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace EmailService.Service
{
    [ExcludeFromCodeCoverage]
    public class Function
    {
        private readonly IServiceCollection serviceCollection;
        private IServiceProvider serviceProvider;

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            serviceCollection = new ServiceCollection();
            ConfigureServices();

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            await ProcessMessageAsync(evnt.Records, context);
        }

        private async Task ProcessMessageAsync(IEnumerable<SQSEvent.SQSMessage> messages, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed {messages.Count()} messages");

            try
            {
                var message = messages.First().Body;
                await serviceProvider.GetService<EmailServiceProcessor>().Handle(message, context);
            }
            catch (Exception exception)
            {
                context.Logger.LogLine(exception.Message);
                throw;
            }

            await Task.CompletedTask;
        }

        private void ConfigureServices()
        {
            var configuration = GetConfiguration();
            serviceCollection.AddSingleton(x => configuration);

            //Initialize static class
            Config.Init(configuration);

            if (Config.IsLocal)
            {
                serviceCollection.AddSingleton<IQueueService, LocalQueueService>();
            }
            else
            {
                var queueService = new AmazonSQSClient(RegionEndpoint.GetBySystemName(Config.AwsRegionEndpoint));
                serviceCollection.AddSingleton<IAmazonSQS>(q => queueService);
                serviceCollection.AddSingleton<IQueueService, AwsQueueService>();
            }
            

            serviceCollection.AddTransient<IPgDataClient, PostgreSQLDataClient>();
            /*serviceCollection.AddTransient<ITemplateEngine, TemplateEngine>();

            serviceCollection.AddSingleton<ISendGridClient>(x => new SendGridClient(Config.SendGridApiKey));
            serviceCollection.AddTransient<SendGridProvider>();*/

            var amazonNotificationServiceClient = new AmazonSimpleEmailServiceClient(RegionEndpoint.GetBySystemName(Config.AwsRegionEndpoint));
            serviceCollection.AddSingleton<IAmazonSimpleEmailService>(q => amazonNotificationServiceClient);
            serviceCollection.AddSingleton<SimpleEmailServiceProvider>();

            serviceCollection.AddTransient<IEmailProviderService, EmailProviderService>();

            serviceCollection.AddTransient<EmailServiceProcessor>();

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{Config.EnvironmentName}.json", true);

            return builder.Build();
        }
    }
}
