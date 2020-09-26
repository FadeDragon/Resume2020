using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailService.Service.Services
{
    public class SendGridProvider : IEmailProvider
    {
        private readonly ISendGridClient sendGridClient;
        private readonly Regex emailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        public int Id { get; set; }

        public SendGridProvider(ISendGridClient sendGridClient)
        {
            this.sendGridClient = sendGridClient;
        }

        public async Task<string> SendAsync(string fromEmail, string toEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(fromEmail) && !emailRegex.IsMatch(fromEmail))
            {
                throw new ArgumentNullException($"Invalid From Email");
            }

            if (string.IsNullOrEmpty(toEmail) && !emailRegex.IsMatch(toEmail))
            {
                throw new ArgumentNullException($"Invalid To Email");
            }

            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentNullException($"Invalid Subject");
            }

            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentNullException($"Invalid Body");
            }

            var from = new EmailAddress(fromEmail);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, body);

            var response = await sendGridClient.SendEmailAsync(msg);

            if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
            {
                return response.Headers.GetValues("X-Message-Id").FirstOrDefault();
            }

            var ex = $"SendGrid Exception: {response.StatusCode} {response.StatusCode:G}\n{await response.Body.ReadAsStringAsync()}";
            throw new Exception(ex);
        }
    }
}
