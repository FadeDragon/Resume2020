using System.Threading.Tasks;
using EmailService.Service.Models;

namespace EmailService.Service.Services
{
    public interface IEmailProviderService
    {
        //ValueTask<Provider> GetEmailProviderAsync();
        Provider GetEmailProviderAsync();
    }
}
