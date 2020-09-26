using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace EmailService.Core.Queue
{
    public interface IQueueService
    {
        Task<bool> SendMessage(string message, string queueUrl);
        Task<ReceiveMessageResponse> ReceiveMessageAsync(string queueUrl, int? maxNumberOfMessages = null, int? visibilityTimeout = null, int? waitTimeSeconds = null);
        Task<bool> DeleteMessage(string messageId, string receiptHandle, string queueUrl);
        Task<bool> DeleteBatchMessages(List<DeleteMessageBatchRequestEntry> entries, string queueUrl);
    }
}
