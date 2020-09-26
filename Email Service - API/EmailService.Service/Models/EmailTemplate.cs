using System.Collections.Generic;
using Newtonsoft.Json;

namespace EmailService.Service.Models
{
    public class EmailTemplate
    {
        public string Subject { get; set; }

        public string ApplicationTemplateBodyHtml { get; set; }

        public Dictionary<string, object> ApplicationTemplateText { get; set; }

        public string EmailTemplateBodyHtml { get; set; }

        public Dictionary<string, object> EmailTemplateText { get; set; }

        public override string ToString()
        {
            return $@"{Subject}\n
                      ApplicationTemplate[\n{ApplicationTemplateBodyHtml}\n{JsonConvert.SerializeObject(ApplicationTemplateText)}\n] 
                      EmailTemplate [\n{EmailTemplateBodyHtml}\n{JsonConvert.SerializeObject(EmailTemplateText)}\n]";
        }
    }
}
