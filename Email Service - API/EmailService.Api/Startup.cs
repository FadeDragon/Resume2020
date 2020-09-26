using System;
using System.Diagnostics.CodeAnalysis;
using EmailService.Core;
using EmailService.Core.Queue;
using EmailService.Core.Data;
using Amazon;
using Amazon.SQS;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EmailService.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private static IConfiguration Configuration { get; set; }
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Startup(IWebHostEnvironment environment)
        {
            var env = Environment.GetEnvironmentVariable("Environment") ?? environment.EnvironmentName;

            var builder = new ConfigurationBuilder().SetBasePath(environment.ContentRootPath)
                                                    .AddJsonFile("appsettings.json", false, true)
                                                    .AddJsonFile($"appsettings.{env}.json", true)
                                                    .AddEnvironmentVariables();

            Configuration = builder.Build();

            Config.Init(Configuration);
        }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(x => Configuration);
            /*services.AddRaygun(Configuration, new RaygunMiddlewareSettings
            {
                ClientProvider = new RaygunClientProvider()
            });*/

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddTransient<IPgDataClient, PostgreSQLDataClient>();

            services.Configure<ConnectionStringsConfig>(Configuration.GetSection("ConnectionStrings"));

            AddQueueService(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "development")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //app.UseRaygun();
            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private static void AddQueueService(IServiceCollection services)
        {
            if (Config.IsLocal)
            {
                services.AddSingleton<IQueueService, LocalQueueService>();
            }
            else
            {
                var amazonSqs = new AmazonSQSClient(RegionEndpoint.GetBySystemName(Config.AwsRegionEndpoint));
                services.AddSingleton<IAmazonSQS>(q => amazonSqs);

                services.AddSingleton<IQueueService, AwsQueueService>();
            }
        }
    }
}
