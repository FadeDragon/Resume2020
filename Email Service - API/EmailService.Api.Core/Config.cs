using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace EmailService.Core
{
    [ExcludeFromCodeCoverage]
    public class Config
    {
        /// <summary>
        /// Private fields
        /// </summary>
        private static IConfiguration configuration;

        public static void Init(IConfiguration config)
        {
            configuration = config;
        }

        public static string EnvironmentName => Environment.GetEnvironmentVariable("Environment") ?? "Development";

        public static bool IsLocal => EnvironmentName.ToLower() == "development";

        /// <summary>
        /// AWS Client Setting
        /// </summary>
        public static string AwsRegionEndpoint => configuration.GetSection("AwsRegionEndpoint").Value;
        public static string EmailServiceSqsUrl => configuration.GetSection("EmailServiceSqsUrl").Value;

        /// <summary>
        /// Raygun Key
        /// </summary>
        public static string RayGunApiKey => configuration.GetSection("RayGunApiKey").Value;

        /// <summary>
        /// SendGrid API Key
        /// </summary>
        public static string SendGridApiKey => configuration.GetSection("SendGridApiKey").Value;
    }
}
