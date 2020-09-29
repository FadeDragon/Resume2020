using System;
using EmailService.Core.Data;

namespace EmailService.Service.Queries
{
    public class UpdateNotificationRequestCompletedAtCommand : DataQueryBase
    {
        public UpdateNotificationRequestCompletedAtCommand(Guid id)
        {
            Parameters.Add("id", id);

            CmdText = @"
UPDATE public.notification_request SET completed_at = CURRENT_TIMESTAMP, notification_status_id = 3 WHERE id = @id;
";
        }
    }
}