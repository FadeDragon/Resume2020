using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmailService.Api.Models.Validators
{
    public class ContainsRequiredAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var dict = value as IDictionary<string, string>;
            if (dict == null)
            {
                return false;
            }

            if (!dict.TryGetValue("ApplicationName", out var appId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(appId))
            {
                return false;
            }

            if (!dict.TryGetValue("MachineName", out var macName))
            {
                return false;
            }

            if (string.IsNullOrEmpty(macName))
            {
                return false;
            }

            return true;
        }
    }
}
