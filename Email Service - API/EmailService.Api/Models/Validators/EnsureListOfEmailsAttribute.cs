using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using EmailService.Core.Models;

namespace EmailService.Api.Models.Validators
{
    public class EnsureListOfEmailsAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var list = value as IEnumerable<Recipient>;
            if (list == null)
            {
                return false;
            }

            foreach (var element in list)
            {
                // for each recipient, email and send code is required
                try
                {
                    var email = new MailAddress(element.Email, !string.IsNullOrEmpty(element.Name) ? element.Name : null);
                }
                catch
                {
                    ErrorMessage += $"Recipient list : {element.Email} is not in a valid email format";
                    return false;
                }

                if (element.SendCode == SendCode.None)
                {
                    ErrorMessage += $"Recipient list : {element.Email} must have a SendCode, currently it is {element.SendCode.ToString()}";
                    return false;
                }
            }

            return true;
        }
    }
}
