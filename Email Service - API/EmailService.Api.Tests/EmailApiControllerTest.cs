using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using EmailService.Api.Controllers;
using EmailService.Api.Models;
using EmailService.Core.Models;
using EmailService.Api.Queries;
using EmailService.Core;
using EmailService.Core.Data;
using EmailService.Core.Queue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace EmailService.Api.Tests
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class EmailApiControllerTest
    {
        private Mock<IConfiguration> _configuration;
        private Mock<IPgDataClient> _pgClient;
        private Mock<IQueueService> _queueService;

        private EmailApiController testEmailController;

        private readonly SendNotificationRequest standardGoodRequest = new SendNotificationRequest
        {
            ApplicationId = 1,
            NotificationTypeId = new Guid("00000000-0000-0000-0000-000000000002"),
            CountryCode = "test",
            FromEmail = "test@test.com",
            Attributes = new Dictionary<string, string>
            {
                { "ApplicationName" , "test" },
                { "MachineName" , "test" }
            },
            RecipientList = new List<Recipient>
            {
                new Recipient { Email= "recipient@test.com", SendCode = SendCode.To }
            }
        };

        [SetUp]
        public void Setup()
        {
            _configuration = new Mock<IConfiguration>();
            _pgClient = new Mock<IPgDataClient>();
            _queueService = new Mock<IQueueService>();

            _configuration.Setup(x => x.GetSection(It.Is<string>(s => s == "EmailServiceSqsUrl")))
                          .Returns(new FakeConfiguration { Value = "testQ" });
            Config.Init(_configuration.Object);

            testEmailController = new EmailApiController(_pgClient.Object, _queueService.Object);
        }

        [Test, Category("Unit")]
        public async Task On_Successful_Send_Should_Respond_With_New_Guid_For_Request()
        {
            // arrange
            var goodRequest = new SendNotificationRequest
            {
                ApplicationId = 1,
                NotificationTypeId = new Guid("00000000-0000-0000-0000-000000000002"),
                CountryCode = "test",
                FromEmail = "test@test.com",
                Attributes = new Dictionary<string, string>
                {
                    { "ApplicationName" , "test" },
                    { "MachineName" , "test" }
                },
                RecipientList = new List<Recipient>
                {
                    new Recipient
                    {
                        Email = "recipient@test.com",
                        Name = "recipient 1",
                        SendCode = SendCode.To
                    },
                    new Recipient
                    {
                        Email = "recipient2@test.com",
                        Name = "recipient 2",
                        SendCode = SendCode.CC
                    },
                    new Recipient
                    {
                        Email = "recipient3@test.com",
                        Name = "recipient 3",
                        SendCode = SendCode.BCC
                    }
                }
            };

            _pgClient.Setup(x => x.Insert(
                            It.IsAny<InsertNotificationRequest>(),
                            It.IsAny<NotificationRequest>(),
                            It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);

            _queueService.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(true);

            // act
            var result = await testEmailController.Send(goodRequest);

            // assert
            _pgClient.Verify(x => x.Insert(
                             It.IsAny<InsertNotificationRequest>(),
                             It.Is<NotificationRequest>(
                                request => request.RecipientList.Any(recipient =>
                                recipient.Email.Equals("recipient2@test.com") && recipient.SendCode == SendCode.CC)),
                             It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            Assert.AreNotEqual(((SendNotificationResponse)((OkObjectResult)result.Result).Value).RequestId, Guid.Empty);
        }

        [Test, Category("Unit")]
        public async Task On_Failure_From_DataBase_Should_Warn_The_Caller()
        {
            // arrange
            _pgClient.Setup(x => x.Insert(
                            It.IsAny<InsertNotificationRequest>(),
                            It.IsAny<NotificationRequest>(),
                            It.IsAny<CancellationToken>()))
                     .ReturnsAsync(0);

            _queueService.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(false);

            // act
            var result = await testEmailController.Send(standardGoodRequest);

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            Assert.AreEqual(((ObjectResult)result.Result).StatusCode, StatusCodes.Status503ServiceUnavailable);
            Assert.IsTrue(((SendNotificationResponse)((ObjectResult)result.Result).Value)
                          .ValidationResult.Contains("Send failed"),
                          "Should have warned the caller that the database could not record the request");
        }

        [Test, Category("Unit")]
        public async Task If_Send_Request_Contains_Invalid_ApplicationOrType_ID_Should_Not_Throw()
        {
            // arrange
            // cannot new SQLException("...")
            _pgClient.Setup(x => x.Insert(
                            It.IsAny<InsertNotificationRequest>(),
                            It.IsAny<NotificationRequest>(),
                            It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new ExternalException("Application ID violates foreign key constraint in request table"));

            _queueService.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(false);

            // act
            var result = await testEmailController.Send(standardGoodRequest);

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            Assert.AreEqual(((ObjectResult)result.Result).StatusCode, StatusCodes.Status400BadRequest);
            Assert.IsTrue(((SendNotificationResponse)((ObjectResult)result.Result).Value)
                          .ValidationResult.Contains("violates a foreign key"),
                          "Should have warned the caller that the database could not record the request");
        }

        [Test, Category("Unit")]
        public void If_Send_Cannot_Connect_To_Postgres_Should_Throw()
        {
            // arrange
            _pgClient.Setup(x => x.Insert(
                            It.IsAny<InsertNotificationRequest>(),
                            It.IsAny<NotificationRequest>(),
                            It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Connection refused or timed out"));

            _queueService.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(false);

            // act
            // assert
            Assert.ThrowsAsync<Exception>(() => testEmailController.Send(standardGoodRequest),
                                          "Should rethrow connection exceptions");
        }

        [Test, Category("Unit")]
        public async Task On_Failure_From_Queue_Service_Should_Warn_The_Caller()
        {
            // arrange
            _pgClient.Setup(x => x.Insert(
                            It.IsAny<InsertNotificationRequest>(),
                            It.IsAny<NotificationRequest>(),
                            It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);

            _queueService.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(false);

            // act
            var result = await testEmailController.Send(standardGoodRequest);

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            Assert.AreEqual(((ObjectResult)result.Result).StatusCode, StatusCodes.Status503ServiceUnavailable);
            Assert.IsTrue(((SendNotificationResponse)((ObjectResult)result.Result).Value)
                          .ValidationResult.Contains("Send failed"),
                          "Should have warned the caller that the queue could not accept the message");
        }

        [Test, Category("Unit")]
        public async Task On_Success_Check_Should_Return_Ok()
        {
            // arrange
            _pgClient.Setup(x => x.FirstOrDefault<FindNotificationRequestQuery.Result>(It.IsAny<FindNotificationRequestQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FindNotificationRequestQuery.Result
                     {
                         NotificationStatusId = 1,
                     });

            // act
            var result = await testEmailController.Check(Guid.Empty);

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            Assert.That(((ObjectResult)result.Result).StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test, Category("Unit")]
        public async Task On_Failure_Check_Should_Return_NotFound()
        {
            // arrange
            _pgClient.Setup(x => x.FirstOrDefault<FindNotificationRequestQuery.Result>(It.IsAny<FindNotificationRequestQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((FindNotificationRequestQuery.Result)null);

            // act
            var result = await testEmailController.Check(Guid.Empty);

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }
    }
}
