using System;

namespace EmailService.Api.Models
{
    public class SendNotificationResponse
    {
        public Guid RequestId { get; set; }

        public string ValidationResult { get; set; }
    }
}
