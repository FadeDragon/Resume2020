using System.Threading.Tasks;

namespace EmailService.Service.Services
{
    public interface IEmailProvider
    {
        Task<string> SendAsync(string fromEmail, string toEmail, string subject, string body);

        int Id { get; set; }
    }
}
