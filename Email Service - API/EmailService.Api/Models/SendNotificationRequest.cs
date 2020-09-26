using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using EmailService.Api.Models.Validators;
using EmailService.Core.Models;

namespace EmailService.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class SendNotificationRequest
    {
        public int ApplicationId { get; set; }

        public Guid NotificationTypeId { get; set; }

        public string CountryCode { get; set; }

        [EmailAddress]
        public string FromEmail { get; set; }

        public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, object> RequestData { get; set; } = new Dictionary<string, object>();

        [Required, EnsureListOfEmails]
        public IEnumerable<Recipient> RecipientList { get; set; }
    }
}
