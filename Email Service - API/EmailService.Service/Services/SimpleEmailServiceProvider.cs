using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace EmailService.Service.Services
{
    public class SimpleEmailServiceProvider : IEmailProvider
    {
        private readonly IAmazonSimpleEmailService sesClient;
        private readonly Regex emailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        public int Id { get; set; }
        public SimpleEmailServiceProvider(IAmazonSimpleEmailService awsSimpleEmailServiceClient)
        {
            sesClient = awsSimpleEmailServiceClient;
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

            var sendRequest = new SendEmailRequest
            {
                Source = fromEmail,
                Destination = new Destination
                {
                    ToAddresses = { toEmail }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = string.IsNullOrEmpty(body) ? null : new Content
                        {
                            Charset = "UTF-8",
                            Data = body
                        }
                    }
                }
            };

            var response = await sesClient.SendEmailAsync(sendRequest);
            return response.MessageId;
        }
    }
}
