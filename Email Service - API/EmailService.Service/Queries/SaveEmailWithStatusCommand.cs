using System;
using EmailService.Core.Data;
using EmailService.Service.Models;

namespace EmailService.Service.Queries
{
    public class SaveEmailWithStatusCommand : DataQueryBase
    {
        public SaveEmailWithStatusCommand(Guid notificationRequestId, string toAddress, 
                                          EmailStatus emailStatus, string subject = null,  
                                          int? providerId = null, string providerMessageId = null)
        {
            Parameters.Add("Id", Guid.NewGuid());
            Parameters.Add("notificationRequestId", notificationRequestId);
            Parameters.Add("providerId", providerId);
            Parameters.Add("emailStatusId", Convert.ToInt32(emailStatus));
            Parameters.Add("toAddress", toAddress);
            Parameters.Add("providerMessageId", providerMessageId);
            Parameters.Add("subject", subject);

            CmdText = @"
INSERT INTO public.email
(id, notification_request_id, provider_id, email_status_id, to_address, provider_message_id, subject) VALUES 
(@Id, @notificationRequestId, @providerId, @emailStatusId, @toAddress, @providerMessageId, @subject)
";
        }
    }
}