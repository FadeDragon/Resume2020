using System.Diagnostics.CodeAnalysis;
using EmailService.Core.Data;

namespace EmailService.Api.Queries
{
    [ExcludeFromCodeCoverage]
    public class InsertNotificationRequest : DataQueryBase
    {
        public InsertNotificationRequest()
        {
            CmdText = @"
-- Insert notifications for later use
insert into public.notification_request
    (id, application_id, notification_status_id, notification_type_id, country_code, from_email, attributes, request_data, recipient_list)
values
    (@id, @applicationid, @notificationstatusid, @notificationtypeid, @countrycode, @fromemail, @attributes::hstore, @requestdata::jsonb, @recipientlist::jsonb);
";
        }
    }
}
