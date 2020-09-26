using System;
using System.Diagnostics.CodeAnalysis;
using EmailService.Core.Data;

namespace EmailService.Service.Queries
{
    [ExcludeFromCodeCoverage]
    public class RetrieveNotificationRequest : DataQueryBase
    {
        public RetrieveNotificationRequest(Guid Id)
        {
            Parameters.Add("id", Id);

            CmdText = @"
-- Find notification that matches the id
SELECT * FROM public.notification_request
WHERE id = @id;
";
        }
    }
}
