using System.Net.Http;
using Autofac;
using Diagnostics.Jobs.SlackMessageSender;

namespace Diagnostics.Service
{
    /// <summary>
    ///     All IoC registrations
    /// </summary>
    public class ContainerRegistrationsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // PostgreSQL / SQL Server clients are registered undeer the same interface, just ensure your 
            // constructor uses the following in the parameter name to resolve the correct client
            // 'postgresql' in the name for Postgresql
            // 'mssql' in the name for MSSQLClient

            builder.RegisterType<HttpClient>().AsSelf().SingleInstance();
            builder.RegisterType<SlackMessageSender>().AsImplementedInterfaces();
        }
    }
}
