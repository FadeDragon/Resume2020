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
using Moq;
using NUnit.Framework;

namespace EmailService.Service.Tests
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class EmailServiceProcessorTest
    {
        private Mock<IPgDataClient> pgDataClient;
        private Mock<IEmailProvider> emailProvider;
        private Mock<IEmailProviderService> emailProviderService;
        private Mock<ILambdaContext> mockLambdaContext;
        private Mock<FakeLogger> mockContextLogger;
        private EmailServiceProcessor emailServiceProcessor;

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

        private Provider sesEmailProvider;

        [SetUp]
        public void Setup()
        {
            pgDataClient = new Mock<IPgDataClient>();
            emailProvider = new Mock<IEmailProvider>();
            emailProviderService = new Mock<IEmailProviderService>();
            mockLambdaContext = new Mock<ILambdaContext>();
            mockContextLogger = new Mock<FakeLogger>();
            emailServiceProcessor = new EmailServiceProcessor(pgDataClient.Object,
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
                                .ReturnsAsync(default(Service.Models.Provider));

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
                        .ReturnsAsync(sesEmailProvider);

            // Act
            Assert.DoesNotThrowAsync(() => emailServiceProcessor.Handle("{ \"RequestId\" : \"00000000-0000-0000-0000-000000000001\" }", mockLambdaContext.Object));

            // Assert
            emailProvider.Verify(ep =>
                                 ep.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                                 Times.Once);
        }
    }
}
