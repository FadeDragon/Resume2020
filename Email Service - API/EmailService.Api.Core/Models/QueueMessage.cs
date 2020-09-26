using System;
using System.Diagnostics.CodeAnalysis;

namespace EmailService.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class QueueMessage
    {
        public Guid RequestId { get; set; }
    }
}
