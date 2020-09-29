using System.Net.Http;
using Autofac;
using Diagnostics.Jobs.SlackMessageSender;

namespace Diagnostics.Service
{
    /// <summary>
    ///   All IoC registrations
    /// </summary>
    public class ContainerRegistrationsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // These are common functions needed by most jobs
            builder.RegisterType<HttpClient>().AsSelf().SingleInstance();
            builder.RegisterType<SlackMessageSender>().AsImplementedInterfaces();
        }
    }
}
