using System;
using EmailService.Core.Data;

namespace EmailService.Service.Queries
{
    public class UpdateNotificationRequestStartedAtCommand : DataQueryBase
    {
        public UpdateNotificationRequestStartedAtCommand(Guid id)
        {
            Parameters.Add("id", id);

            CmdText = @"
UPDATE public.notification_request SET started_at = CURRENT_TIMESTAMP, notification_status_id = 2 WHERE id = @id;
";
        }
    }
}