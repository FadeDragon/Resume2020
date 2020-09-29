using System.Threading.Tasks;
using EmailService.Core.Data;
using EmailService.Service.Models;
using EmailService.Service.Queries;

namespace EmailService.Service.Services
{
    public class EmailProviderService : IEmailProviderService
    {
        private readonly IPgDataClient pgDataClient;
        //private readonly SendGridProvider sendGridProvider;
        private readonly SimpleEmailServiceProvider simpleEmailServiceProvider;

        public EmailProviderService(IPgDataClient pgDataClient,
                                    //SendGridProvider sendGridProvider,
                                    SimpleEmailServiceProvider simpleEmailServiceProvider)
        {
            this.pgDataClient = pgDataClient;
            //this.sendGridProvider = sendGridProvider;
            this.simpleEmailServiceProvider = simpleEmailServiceProvider;
        }

        //public async ValueTask<Provider> GetEmailProviderAsync()
        public Provider GetEmailProviderAsync()
        {
            /*var provider = await pgDataClient.FirstOrDefault<Provider>(new GetEmailProviderQuery());

            if (provider == null)
            {
                return null;
            }

            switch (provider.Id)
            {
                case 1:
                    provider.EmailProvider = sendGridProvider;
                    break;
                case 2:
                    provider.EmailProvider = simpleEmailServiceProvider;
                    break;
            }*/

            return new Provider { Name = "SES", EmailProvider = simpleEmailServiceProvider };
        }
    }
}
