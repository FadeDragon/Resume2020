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
using EmailService.Service.Templating;

namespace EmailService.Service
{
    /// <summary>
    ///   Handles the retrieval of email requests and processing them for sending via providers.
    /// </summary>
    public class EmailServiceProcessor
    {
        private readonly IPgDataClient pgDataClient;
        private readonly ITemplateEngine templateEngine;
        private readonly IEmailProviderService emailProviderFactory;
        public EmailServiceProcessor(IPgDataClient pgDataClient,
                                     ITemplateEngine templateEngine,
                                     IEmailProviderService emailProviderFactory)
        {
            this.pgDataClient = pgDataClient;
            this.templateEngine = templateEngine;
            this.emailProviderFactory = emailProviderFactory;
        }

        public async Task Handle(string messageBody, ILambdaContext context)
        {
            // id in message body is the id of a request in database
            context.Logger.LogLine($"EmailServiceProcessor : Started on {messageBody}");
            var request = await RetrieveRequestFromDatabase(messageBody);

            // get email provider based on configuration
            var provider = GetEmailProvider(request);
            context.Logger.LogLine($"Using email provider {provider.Id} : {provider.Name}");

            // update started_at of request
            await pgDataClient.Execute(new UpdateNotificationRequestStartedAtCommand(request.Id));
            
            // process for each recipient
            foreach (var recipient in request.RecipientList)
            {
                await ProcessEmailForRecipient(request, recipient, provider, context);
            }
            
            // update completed_at
            await pgDataClient.Execute(new UpdateNotificationRequestCompletedAtCommand(request.Id));
        }

        private async Task<NotificationRequest> RetrieveRequestFromDatabase(string messageBody)
        {
            var queueMsq = JsonConvert.DeserializeObject<QueueMessage>(messageBody);

            return await pgDataClient.FirstOrDefault<NotificationRequest>(new RetrieveNotificationRequest(queueMsq.RequestId));
        }

        private Provider GetEmailProvider(NotificationRequest request)
        {
            var provider = emailProviderFactory.GetEmailProviderAsync();
            if (provider == null)
            {
                throw new Exception("No email provider available.");
            }

            return provider;
        }

        private async Task ProcessEmailForRecipient(NotificationRequest request, Recipient recipient, Provider provider, ILambdaContext context)
        {
            context.Logger.LogLine($"Notification Request : {JsonConvert.SerializeObject(request)}");
            
            // countryCode = 'nz', 'uk' etc. and languageCode complies with ISO-639-2
            var emailTemplateResult = await pgDataClient.FirstOrDefault<GetEmailTemplateQuery.Result>(new GetEmailTemplateQuery(
                request.NotificationTypeId,
                request.CountryCode,
                recipient.Language));
            
            if (emailTemplateResult == null)
            {
                context.Logger.LogLine($"No email template available for NotificationTypeId: {request.NotificationTypeId}, CountryCode: {request.CountryCode}, Language: {recipient.Language}");
                return;
            }
            
            var emailTemplate = new EmailTemplate
            {
                Subject = emailTemplateResult.Subject,
                ApplicationTemplateBodyHtml = emailTemplateResult.ApplicationTemplateBodyHtml,
                ApplicationTemplateText = JsonConvert.DeserializeObject<Dictionary<string, object>>(emailTemplateResult.ApplicationTemplateText),
                EmailTemplateBodyHtml = emailTemplateResult.EmailTemplateBodyHtml,
                EmailTemplateText = JsonConvert.DeserializeObject<Dictionary<string, object>>(emailTemplateResult.EmailTemplateText)
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

                await SaveEmailStatusWithProvider(EmailStatus.SentToProvider, request, email, provider, providerMessageId);
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Email {request.Id} with recipient {email.ToEmail} failed sent to email provider ({provider.Id})\nError: {ex}");
                
                await SaveEmailStatusWithProvider(EmailStatus.FailedToSendToProvider, request, email, provider);
            }
        }

        private Email EmailTemplateAndDataMerge(EmailTemplate emailTemplateAndData)
        {
            // translations done on templates and data
            
            // use template engine to render variables with values within email templates
            var appTemplate = templateEngine.Render(emailTemplateAndData.ApplicationTemplateBodyHtml, emailTemplateAndData.ApplicationTemplateText);

            var emailTemplate = templateEngine.Render(emailTemplateAndData.EmailTemplateBodyHtml, emailTemplateAndData.EmailTemplateText);

            var body = appTemplate.Replace("<email-template/>", emailTemplate);

            var mergedEmail = new Email
            {
                Subject = emailTemplateAndData.Subject,
                Body = body
            };

            return mergedEmail;
        }

        private async Task SaveEmailStatusWithProvider(EmailStatus status, NotificationRequest request,
                                     Email email, Provider provider, string providerMessageId = "")
        {
            await pgDataClient.Execute(new SaveEmailWithStatusCommand(request.Id,
                                                                            email.ToEmail, 
                                                                            status, 
                                                                            email.Subject, 
                                                                            provider.Id,
                                                                            providerMessageId));
        }
    }
}