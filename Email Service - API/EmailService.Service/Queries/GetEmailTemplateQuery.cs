using System;
using EmailService.Core.Data;

namespace EmailService.Service.Queries
{
    public class GetEmailTemplateQuery : DataQueryBase
    {
        public GetEmailTemplateQuery(Guid notificationTypeId, string countryCode, string language)
        {
            Parameters.Add("notificationTypeId", notificationTypeId);
            Parameters.Add("countryCode", countryCode);
            Parameters.Add("language", language);

            CmdText = @"
WITH application_template AS (
	SELECT 
		notification_type_id,
		applicationTemplateBodyHtml,
		jsonb_merge_all(applicationTemplateText) applicationTemplateText
	FROM
		public.application_template at
	WHERE at.language = @language
		AND at.notification_type_id = @notificationTypeId
	ORDER BY at.language
		AND at.notification_type_id
	GROUP BY notification_type_id, applicationTemplateBodyHtml
	LIMIT 1
),
email_template AS(
    SELECT
		notification_type_id,
		subject,
		emailTemplateBodyHtml,
		jsonb_merge_all(emailTemplateText) emailTemplateText
	FROM 
		public.email_template et
	WHERE et.language = @language
		AND et.notification_type_id = @notificationTypeId
		AND et.country_code = @countryCode
	ORDER BY et.language
		AND et.notification_type_id
		AND et.country_code
	GROUP BY notification_type_id, subject, emailTemplateBodyHtml
	LIMIT 1
)
SELECT
    email_template.subject,
    application_template.applicationTemplateBodyHtml,
    application_template.applicationTemplateText,
    email_template.emailTemplateBodyHtml,
    email_template.emailTemplateText
FROM
    application_template
    JOIN email_template
    ON application_template.notification_type_id = email_template.notification_type_id
"; 
        }
        
        public class Result
        {
            public string Subject { get; set; }

            public string ApplicationTemplateBodyHtml { get; set; }

            public string ApplicationTemplateText { get; set; }

            public string EmailTemplateBodyHtml { get; set; }

            public string EmailTemplateText { get; set; }
        }
    }
}