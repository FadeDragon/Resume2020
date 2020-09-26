using EmailService.Core.Data;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EmailService.Api.Queries
{
    [ExcludeFromCodeCoverage]
    public class FindNotificationRequestQuery : DataQueryBase
    {
        public FindNotificationRequestQuery(Guid Id)
        {
            Parameters.Add("id", Id);

            CmdText = @"
-- Find notification that matches the id
select notification_status_id, created_at, started_at, completed_at 
from public.notification_request
where id = @id;
";
        }

        public class Result
        {
            public int NotificationStatusId { get; set; }

            public DateTime CreatedAt { get; set; }

            public DateTime? StartedAt { get; set; }

            public DateTime? CompletedAt { get; set; }
        }
    }
}
