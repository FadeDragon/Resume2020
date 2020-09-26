using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using EmailService.Core.Data;
using EmailService.Core.Models;
using EmailService.Service.Models;
using EmailService.Service.Queries;
using Newtonsoft.Json;
using System.Threading.Tasks;
using EmailService.Service.Services;

namespace EmailService.Service
{
    public class EmailServiceProcessor
    {
        private readonly IPgDataClient pgDataClient;
        private readonly IEmailProviderService emailProviderFactory;
        public EmailServiceProcessor(IPgDataClient pgDataClient,
                                     IEmailProviderService emailProviderFactory)
        {
            this.pgDataClient = pgDataClient;
            this.emailProviderFactory = emailProviderFactory;
        }

        public async Task Handle(string messageBody, ILambdaContext context)
        {
            //Id in message body refers to actual request in database
            context.Logger.LogLine($"EmailServiceProcessor : Started on {messageBody}");
            var request = await RetrieveRequestFromDatabase(messageBody);
            context.Logger.LogLine($"Notification Request : {JsonConvert.SerializeObject(request)}");

            // Get email provider based on availability and priority
            var provider = await GetEmailProvider(request);
            context.Logger.LogLine($"Using email provider {provider.Id} : {provider.Name}");

            // Loop through each recipient and process
            foreach (var recipient in request.RecipientList)
            {
                await ProcessEmailForRecipient(request, recipient, provider, context);
            }
        }

        private async Task<NotificationRequest> RetrieveRequestFromDatabase(string messageBody)
        {
            var queueMsq = JsonConvert.DeserializeObject<QueueMessage>(messageBody);

            return await pgDataClient.FirstOrDefault<NotificationRequest>(new RetrieveNotificationRequest(queueMsq.RequestId));
        }

        private async ValueTask<Provider> GetEmailProvider(NotificationRequest request)
        {
            var provider = await emailProviderFactory.GetEmailProviderAsync();
            if (provider == null)
            {
                throw new Exception("No email provider available.");
            }

            return provider;
        }

        private async Task ProcessEmailForRecipient(NotificationRequest request, Recipient recipient, Provider provider, ILambdaContext context)
        {
            var emailTemplate = new EmailTemplate
            {
                Subject = "test email from EmailService",
                ApplicationTemplateBodyHtml = "",
                ApplicationTemplateText = null,
                EmailTemplateBodyHtml = "<h1>Hello World</h1>",
                EmailTemplateText = null
            };

            // Merge data with template
            var email = EmailTemplateAndDataMerge(emailTemplate);
            email.ToEmail = recipient.Email;

            try
            {
                context.Logger.LogLine($"Sending email {request.Id} to {email.ToEmail}");

                var providerMessageId = await provider.EmailProvider.SendAsync(request.FromEmail, email.ToEmail,
                                                                               email.Subject, email.Body);

                context.Logger.LogLine($"Email {request.Id} with recipient {email.ToEmail} successfully sent to email provider ({provider.Id})");
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Email {request.Id} with recipient {email.ToEmail} failed sent to email provider ({provider.Id})\nError: {ex}");
            }
        }

        private Email EmailTemplateAndDataMerge(EmailTemplate emailTemplateAndData)
        {
            //var appTemplate = templateEngine.Render(emailTemplateAndData.ApplicationTemplateBodyHtml, emailTemplateAndData.ApplicationTemplateText);

            //var emailTemplate = templateEngine.Render(emailTemplateAndData.EmailTemplateBodyHtml, emailTemplateAndData.EmailTemplateText);

            //var body = appTemplate.Replace("<email-template/>", emailTemplate);
            var body = emailTemplateAndData.EmailTemplateBodyHtml;

            var mergedEmail = new Email
            {
                Subject = emailTemplateAndData.Subject,
                Body = body
            };

            return mergedEmail;
        }
    }
}