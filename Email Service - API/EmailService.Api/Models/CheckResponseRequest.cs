using System;
using System.Diagnostics.CodeAnalysis;

namespace EmailService.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class CheckResponseRequest
    {
        public int NotificationStatusId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string ValidationResult { get; set; }
    }
}
