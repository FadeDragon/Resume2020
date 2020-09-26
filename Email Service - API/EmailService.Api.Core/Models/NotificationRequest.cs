using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EmailService.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class NotificationRequest
    {
        public Guid Id { get; set; }

        public int ApplicationId { get; set; }

        public int NotificationStatusId { get; set; }

        public Guid NotificationTypeId { get; set; }

        public string CountryCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime CompletedAt { get; set; }

        public string FromEmail { get; set; }

        public IDictionary<string, string> Attributes { get; set; }

        public IDictionary<string, object> RequestData { get; set; }

        public IEnumerable<Recipient> RecipientList { get; set; }
    }
}
