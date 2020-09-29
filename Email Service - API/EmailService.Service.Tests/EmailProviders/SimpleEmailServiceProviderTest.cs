using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using EmailService.Core;
using EmailService.Service.Services;
using Moq;
using NUnit.Framework;

namespace EmailService.Service.Tests.EmailProviders
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class SimpleEmailServiceProviderTest
    {
        private IEmailProvider simpleEmailServiceProvider;
        private Mock<IAmazonSimpleEmailService> amazonSimpleEmailService;
        
        [SetUp]
        public void Setup()
        {
            amazonSimpleEmailService = new Mock<IAmazonSimpleEmailService>();
            simpleEmailServiceProvider = new SimpleEmailServiceProvider(amazonSimpleEmailService.Object);
        }
        
        [Test, Category(TestCategories.Unit)]
        public async Task SimpleEmailServiceProvider_Should_Return_MessageId_If_Response_Is_Accepted()
        {
            //Arrange
            var from = "test@groupimd.com";
            var to = "another-test@groupimd.com";
            var subject = "testing";
            var body = "body not found";
            var response = new SendEmailResponse
            {
                MessageId = "TestId"
            };
            amazonSimpleEmailService.Setup(a => a.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            //Act
            var result = await simpleEmailServiceProvider.SendAsync(from, to, subject, body);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(response.MessageId, result);
        }

        [Test, Category(TestCategories.Unit)]
        public void SimpleEmailServiceProvider_Should_Return_Exception_When_Passing_Empty_Or_Invalid_FromEmail()
        {
            //Arrange
            var from = "";
            var to = "another-test@groupimd.com";
            var subject = "testing";
            var body = "body not found";
            var response = new SendEmailResponse
            {
                MessageId = "TestId"
            };
            amazonSimpleEmailService.Setup(a => a.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            //Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => simpleEmailServiceProvider.SendAsync(from, to, subject, body));
        }

        [Test, Category(TestCategories.Unit)]
        public void SimpleEmailServiceProvider_Should_Return_Exception_When_Passing_Empty_Or_Invalid_ToEmail()
        {
            //Arrange
            var from = "test@groupimd.com";
            var to = "";
            var subject = "testing";
            var body = "body not found";
            var response = new SendEmailResponse
            {
                MessageId = "TestId"
            };
            amazonSimpleEmailService.Setup(a => a.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            //Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => simpleEmailServiceProvider.SendAsync(from, to, subject, body));
        }

        [Test, Category(TestCategories.Unit)]
        public void SimpleEmailServiceProvider_Should_Return_Exception_When_Passing_Empty_Subject()
        {
            //Arrange
            var from = "test@groupimd.com";
            var to = "another-test@groupimd.com";
            var subject = "";
            var body = "body not found";
            var response = new SendEmailResponse
            {
                MessageId = "TestId"
            };
            amazonSimpleEmailService.Setup(a => a.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            //Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => simpleEmailServiceProvider.SendAsync(from, to, subject, body));
        }

        [Test, Category(TestCategories.Unit)]
        public void SimpleEmailServiceProvider_Should_Return_Exception_When_Passing_Empty_Body()
        {
            //Arrange
            var from = "test@groupimd.com";
            var to = "another-test@groupimd.com";
            var subject = "testing";
            var body = "";
            var response = new SendEmailResponse
            {
                MessageId = "TestId"
            };
            amazonSimpleEmailService.Setup(a => a.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            //Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => simpleEmailServiceProvider.SendAsync(from, to, subject, body));
        }
    }
}