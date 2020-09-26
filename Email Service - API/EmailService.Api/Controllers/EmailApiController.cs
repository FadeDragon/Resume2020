using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using EmailService.Api.Models;
using EmailService.Core;
using EmailService.Core.Data;
using EmailService.Core.Models;
using EmailService.Api.Queries;
using EmailService.Core.Queue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EmailService.Api.Controllers
{
    [ApiController]
    public class EmailApiController : ControllerBase
    {
        private readonly IPgDataClient pgClient;
        private readonly IQueueService queueService;

        public EmailApiController(IPgDataClient pgClient, IQueueService queueService)
        {
            this.pgClient = pgClient;
            this.queueService = queueService;
        }

        [HttpPost, Route("send")]
        public async Task<ActionResult<SendNotificationResponse>> Send([FromBody] SendNotificationRequest request)
        {
            var responseValue = new SendNotificationResponse();

            // create new guid and insert into postgres table
            var pgDataRow = new NotificationRequest
            {
                Id = Guid.NewGuid(),
                ApplicationId = request.ApplicationId,
                NotificationStatusId = Convert.ToInt32(NotificationStatus.New),
                NotificationTypeId = request.NotificationTypeId,
                CountryCode = request.CountryCode,
                FromEmail = request.FromEmail?.Trim(),
                Attributes = request.Attributes,
                RequestData = request.RequestData,
                RecipientList = request.RecipientList.Select(x => new Recipient
                {
                    Email = x.Email?.Trim(),
                    Name = x.Name,
                    Language = x.Language,
                    SendCode = x.SendCode
                })
            };

            bool success;

            try
            {
                success = await pgClient.Insert(new InsertNotificationRequest(), pgDataRow, CancellationToken.None) > 0;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("violates foreign key"))
                {
                    responseValue.ValidationResult = $"Send failed with : An ID in this request violates a foreign key constraint";
                    return StatusCode(StatusCodes.Status400BadRequest, responseValue);
                }
                LambdaLogger.Log($"Email Api: Send failed when inserting into database: {e.Message}");
                throw;
            }

            if (!success)
            {
                LambdaLogger.Log($"Email Api: Send failed with : Could not insert request into postgres environment name- {Config.EnvironmentName}");

                responseValue.ValidationResult = $"Send failed with : Could not insert request into postgres environment name- {Config.EnvironmentName}";
                return StatusCode(StatusCodes.Status503ServiceUnavailable, responseValue);
            }

            // insert into SQS for processor to pick up
            var newMessage = new QueueMessage
            {
                RequestId = pgDataRow.Id
            };
            var serializedMessage = JsonConvert.SerializeObject(newMessage);
            success = await queueService.SendMessage(serializedMessage, Config.EmailServiceSqsUrl);

            // if not successful warn the caller
            if (!success)
            {
                LambdaLogger.Log($"Email Api: Send failed with : Could not insert message into queue - {Config.EmailServiceSqsUrl}");

                responseValue.ValidationResult = $"Send failed with : Could not insert message into queue";
                return StatusCode(StatusCodes.Status503ServiceUnavailable, responseValue);
            }

            // on success return the Guid so that the caller can use to check the message status
            LambdaLogger.Log($"Email Api: Send succeeded with guid : {serializedMessage}");
            responseValue.RequestId = pgDataRow.Id;
            return Ok(responseValue);
        }


        [HttpGet, Route("check/{id:guid}")]
        public async Task<ActionResult<CheckResponseRequest>> Check(Guid id)
        {
            var result = await pgClient.FirstOrDefault<FindNotificationRequestQuery.Result>(new FindNotificationRequestQuery(id));

            if (result == null)
            {
                return NotFound(new CheckResponseRequest { ValidationResult = "Cannot find a notification request matching the supplied ID" });
            }

            return Ok(new CheckResponseRequest
            {
                NotificationStatusId = result.NotificationStatusId,
                CreatedAt = result.CreatedAt,
                StartedAt = result.StartedAt,
                CompletedAt = result.CompletedAt
            });
        }
    }
}
