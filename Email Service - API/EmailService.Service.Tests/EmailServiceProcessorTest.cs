using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Amazon.Lambda.Core;
using EmailService.Core.Data;
using EmailService.Core.Models;
using EmailService.Service.Models;
using EmailService.Service.Queries;
using EmailService.Service.Services;
using EmailService.Service.Templating;
using Moq;
using NUnit.Framework;

namespace EmailService.Service.Tests
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class EmailServiceProcessorTest
    {
        private Mock<IPgDataClient> pgDataClient;
        private Mock<ITemplateEngine> templateEngine;
        private Mock<IEmailProvider> emailProvider;
        private Mock<IEmailProviderService> emailProviderService;
        private Mock<ILambdaContext> mockLambdaContext;
        private Mock<FakeLogger> mockContextLogger;
        private EmailServiceProcessor emailServiceProcessor;
        private Provider sesEmailProvider;

        private readonly NotificationRequest validNotificationRequest = new NotificationRequest
        {
            Id = new Guid("00000000-0000-0000-0000-000000000001"),
            ApplicationId = 1,
            NotificationTypeId = new Guid("00000000-0000-0000-0000-000000000003"),
            CountryCode = "sg",
            FromEmail = "test@peachvideo.com",
            RecipientList = new List<Recipient>
            {
                new Recipient { Email = "user@test.com", Name = "unit test", Language = "eng", SendCode = SendCode.To }
            }
        };
        
        private readonly GetEmailTemplateQuery.Result validEmailTemplate = new GetEmailTemplateQuery.Result
        {
            Subject = "test",
            ApplicationTemplateBodyHtml = "<div>Logo<email-template/></div>",
            ApplicationTemplateText = "{\"welcome\": \"Some translatable welcome text\"}",
            EmailTemplateBodyHtml = "<p>the email</p>",
            EmailTemplateText = "{}"
        };

        [SetUp]
        public void Setup()
        {
            pgDataClient = new Mock<IPgDataClient>();
            templateEngine = new Mock<ITemplateEngine>();
            emailProvider = new Mock<IEmailProvider>();
            emailProviderService = new Mock<IEmailProviderService>();
            mockLambdaContext = new Mock<ILambdaContext>();
            mockContextLogger = new Mock<FakeLogger>();
            emailServiceProcessor = new EmailServiceProcessor(pgDataClient.Object,
                                                              templateEngine.Object,
                                                              emailProviderService.Object);

            sesEmailProvider = new Provider { Id = 1, Name = "mock SES", Credentials = "", EmailProvider = emailProvider.Object };
            mockLambdaContext.SetupGet<ILambdaLogger>(a => a.Logger)
                             .Returns(mockContextLogger.Object);
        }

        [Test, Category("Unit")]
        public void Should_Throw_If_No_Email_Provider_Found()
        {
            // Arrange
            pgDataClient.Setup(a => a.FirstOrDefault<NotificationRequest>(It.IsAny<RetrieveNotificationRequest>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(validNotificationRequest);

            emailProviderService.Setup(a => a.GetEmailProviderAsync())
                                .Returns(default(Service.Models.Provider));

            // Act
            Assert.ThrowsAsync(typeof(Exception),
                               () => emailServiceProcessor.Handle("{ \"RequestId\" : \"00000000-0000-0000-0000-000000000001\" }", mockLambdaContext.Object));

            // Assert
            emailProvider.Verify(ep =>
                                 ep.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                                 Times.Never);
        }

        [Test, Category("Unit")]
        public void Should_Call_Send_Email_Matching_Recipients_Language()
        {
            // Arrange
            pgDataClient.Setup(a => a.FirstOrDefault<NotificationRequest>(It.IsAny<RetrieveNotificationRequest>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(validNotificationRequest);

            emailProviderService.Setup(a => a.GetEmailProviderAsync())
                        .Returns(sesEmailProvider);
            
            pgDataClient.Setup(a => a.FirstOrDefault<GetEmailTemplateQuery.Result>(It.IsAny<GetEmailTemplateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validEmailTemplate);
            
            templateEngine.Setup(a => a.Render(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns("<div>Logo<email-template/></div>");

            // Act
            Assert.DoesNotThrowAsync(() => emailServiceProcessor.Handle("{ \"RequestId\" : \"00000000-0000-0000-0000-000000000001\" }", mockLambdaContext.Object));

            // Assert
            pgDataClient.Verify(client => 
                                client.FirstOrDefault<GetEmailTemplateQuery.Result>(
                                It.Is<GetEmailTemplateQuery>(q => q.Parameters["language"].Equals("eng")), 
                                It.IsAny<CancellationToken>()), 
                                Times.Once);
            emailProvider.Verify(ep =>
                                 ep.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                                 Times.Once);
        }
    }
}
