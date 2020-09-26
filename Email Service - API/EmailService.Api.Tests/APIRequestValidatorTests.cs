using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using EmailService.Api.Models.Validators;
using EmailService.Core.Models;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;

namespace EmailService.Api.Tests
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class APIRequestValidatorTests
    {
        private readonly ContainsRequiredAttribute containsRequired = new ContainsRequiredAttribute();
        private readonly EnsureListOfEmailsAttribute ensureListOfEmails = new EnsureListOfEmailsAttribute();

        [SetUp]
        public void Setup()
        {
        }


        [Test, Category("Unit")]
        public void ContainsRequired_Validates_Attributes()
        {
            // arrange
            var dict = new Dictionary<string, string>
            {
                {"ApplicationName", "test app"},
                {"MachineName", "test machine"}
            };

            // act
            var result = containsRequired.IsValid(dict);

            // assert
            Assert.IsTrue(result);
        }

        [Test, Category("Unit")]
        public void ContainsRequired_Does_Not_Throw_If_Not_Dictionary()
        {
            // arrange
            var notdict = new List<string> { "ApplicationName" };
            var result = false;

            // act
            // assert
            Assert.DoesNotThrow(() => result = containsRequired.IsValid(notdict));
            Assert.IsFalse(result);
        }

        [TestCase("{}"), Category("Unit")]
        [TestCase("{\"MachineName\":\"test machine\"}"), Category("Unit")]
        [TestCase("{\"ApplicationName\":\"test app\"}"), Category("Unit")]
        public void If_A_Required_Attribute_Missing_ContainsRequired_Returns_False(string input)
        {
            // arrange
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(input);

            // act
            var result = containsRequired.IsValid(dict);

            // assert
            Assert.IsFalse(result);
        }

        [TestCase("{\"ApplicationName\":\"\", \"MachineName\":\"test machine\"}"), Category("Unit")]
        [TestCase("{\"ApplicationName\":\"test app\", \"MachineName\":\"\"}"), Category("Unit")]
        public void If_A_Required_Attribute_Is_Blank_Returns_False(string input)
        {
            // arrange
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(input);

            // act
            var result = containsRequired.IsValid(dict);

            // assert
            Assert.IsFalse(result);
        }

        [TestCase("[{\"Email\":\"recipient@test.com\",\"Name\":\"test\",\"SendCode\":1}]"), Category("Unit")]
        [TestCase("[{\"Email\":\"recipient@test.com\",\"Name\":null,\"SendCode\":2}]"), Category("Unit")]
        public void EnsureListOfEmails_Validates_Proper_Emails(string input)
        {
            // arrange
            var list = JsonConvert.DeserializeObject<List<Recipient>>(input);

            // act
            var result = ensureListOfEmails.IsValid(list);

            // assert
            Assert.IsTrue(result);
        }

        [Test, Category("Unit")]
        public void EnsureListOfEmails_Does_Not_Throw_If_Not_Recipient_List()
        {
            // arrange
            var notListOfRecipients = new List<string> { "email" };
            var result = false;

            // act
            // assert
            Assert.DoesNotThrow(() => result = ensureListOfEmails.IsValid(notListOfRecipients));
            Assert.IsFalse(result);
        }

        [TestCase("[{\"Email\":\"recipient@test.com\",\"Name\":null,\"SendCode\":0}]"), Category("Unit")]
        [TestCase("[{\"Email\":\"recipient.com\",\"Name\":null,\"SendCode\":0}]"), Category("Unit")]
        [TestCase("[{\"Email\":\"recipient@\",\"Name\":null,\"SendCode\":0}]"), Category("Unit")]
        [TestCase("[{\"Email\":\"abc\",\"Name\":null,\"SendCode\":0}]"), Category("Unit")]
        public void If_List_Contains_Nonvalid_Emails_EnsureListOfEmails_Returns_False(string input)
        {
            // arrange
            var list = JsonConvert.DeserializeObject<List<Recipient>>(input);

            // act
            var result = ensureListOfEmails.IsValid(list);

            // assert
            Assert.IsFalse(result);
        }
    }
}
