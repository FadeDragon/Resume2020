using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EmailService.Service.Templating
{
    public class TemplateEngine : ITemplateEngine
    {
        private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline;

        private readonly IList<Func<string, IDictionary<string, object>, string>> processors = new List<Func<string, IDictionary<string, object>, string>>();

        public TemplateEngine()
        {
            processors.Add(PerformIfConditionSubstitutions);
            processors.Add(PerformReplacementSubstitutions);
            processors.Add(PerformForLoopSubstitutions);
        }

        public string Render(string template, IDictionary<string, object> data)
        {
            var result = template;

            foreach (var processor in processors)
            {
                result = processor.Invoke(result, data);
            }

            return result;
        }

        private string PerformIfConditionSubstitutions(string template, IDictionary<string, object> data)
        {
            return template;
        }

        private string PerformReplacementSubstitutions(string template, IDictionary<string, object> data)
        {
            return template;
        }

        private string PerformForLoopSubstitutions(string template, IDictionary<string, object> data)
        {
            return template;
        }
    }
}