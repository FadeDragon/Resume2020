using EmailService.Service.Services;

namespace EmailService.Service.Models
{
    public class Provider
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Credentials { get; set; }

        public IEmailProvider EmailProvider { get; set; }
    }
}
