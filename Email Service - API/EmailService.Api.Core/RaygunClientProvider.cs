using System.Diagnostics.CodeAnalysis;
using Mindscape.Raygun4Net.AspNetCore;

namespace IMD.Cloud.EmailService.Core
{
    [ExcludeFromCodeCoverage]
    public class RaygunClientProvider : DefaultRaygunAspNetCoreClientProvider
    {
        public override RaygunClient GetClient(RaygunSettings settings)
        {
            var client = base.GetClient(settings);

            settings.ApiKey = Config.RayGunApiKey;

            return client;
        }
    }
}
