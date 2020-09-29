using System.Collections.Generic;

namespace EmailService.Service.Templating
{
    public interface ITemplateEngine
    {
        string Render(string template, IDictionary<string, object> data);
    }
}