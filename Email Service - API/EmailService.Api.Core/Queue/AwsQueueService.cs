using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Polly;
using Polly.Timeout;

namespace EmailService.Core.Queue
{
    [ExcludeFromCodeCoverage]
    public class AwsQueueService : IQueueService
    {
        private readonly IAmazonSQS client;

        public AwsQueueService(IAmazonSQS amazonSQSClient)
        {
            client = amazonSQSClient;
        }

        public async Task<bool> SendMessage(string message, string queueUrl)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(queueUrl))
            {
                throw new ArgumentNullException();
            }

            LambdaLogger.Log($"AwsQueueService.SendMessage queueUrl: {queueUrl}");

            var sendMsgRequest = new SendMessageRequest
            {
                MessageBody = message,
                QueueUrl = queueUrl,
                DelaySeconds = 5
            };

            try
            {
                LambdaLogger.Log($"AwsQueueService.SendMessage request: message: {message}");

                var timeoutPolicy = Policy.Timeout(6, TimeoutStrategy.Pessimistic);
                var respond = await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(3,
                        retryAttempt => TimeSpan.FromSeconds(retryAttempt * 0.5),
                        onRetry: (exception, retryAttempt) =>
                        {
                            var exceptionMessage = exception == null ? string.Empty : exception.Message;
                            LambdaLogger.Log(
                                $"AwsQueueService.SendMessage retry {retryAttempt}=> message: {message}, exception:{exceptionMessage}");
                        }).ExecuteAsync(async () => await timeoutPolicy.Execute(async () => await this.client.SendMessageAsync(sendMsgRequest).ConfigureAwait(false)));

                LambdaLogger.Log("AwsQueueService.SendMessage response: " + respond.HttpStatusCode);

                return respond.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log("SendMessageAsync Error on " + queueUrl);
                LambdaLogger.Log("SendMessageAsync Error: " + ex.Message + " " + ex + "\n" + "Inner Exception: " + ex.InnerException);
                throw;
            }
        }

        public async Task<ReceiveMessageResponse> ReceiveMessageAsync(string queueUrl, int? maxNumberOfMessages = null, int? visibilityTimeout = null, int? waitTimeSeconds = null)
        {
            if (string.IsNullOrEmpty(queueUrl))
            {
                throw new ArgumentNullException();
            }

            var receiveMsgRequest = new ReceiveMessageRequest
            {
                MaxNumberOfMessages = maxNumberOfMessages ?? 10,
                VisibilityTimeout = visibilityTimeout ?? 60,
                WaitTimeSeconds = waitTimeSeconds ?? 10,
                QueueUrl = queueUrl
            };

            return await client.ReceiveMessageAsync(receiveMsgRequest, CancellationToken.None);
        }

        public async Task<bool> DeleteMessage(string messageId, string receiptHandle, string queueUrl)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(receiptHandle))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(queueUrl))
            {
                throw new ArgumentNullException();
            }

            var deleteMessageRequest = new DeleteMessageRequest
            {
                ReceiptHandle = receiptHandle,
                QueueUrl = queueUrl
            };

            var respond = await client.DeleteMessageAsync(deleteMessageRequest);

            return respond.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DeleteBatchMessages(List<DeleteMessageBatchRequestEntry> entries, string queueUrl)
        {
            if (entries == null || entries.Count == 0)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(queueUrl))
            {
                throw new ArgumentNullException();
            }

            var deleteMsgBatchRequest = new DeleteMessageBatchRequest
            {
                Entries = entries,
                QueueUrl = queueUrl
            };

            var respond = await client.DeleteMessageBatchAsync(deleteMsgBatchRequest, CancellationToken.None);

            return respond.HttpStatusCode == HttpStatusCode.OK;
        }
    }
}
