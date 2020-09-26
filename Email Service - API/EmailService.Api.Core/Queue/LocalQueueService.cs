using Amazon.SQS.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EmailService.Core.Queue
{
    [ExcludeFromCodeCoverage]
    public class LocalQueueService : IQueueService
    {
        private static readonly Queue<string> Queue = new Queue<string>();

        public Task<bool> SendMessage(string message, string queueUrl)
        {
            return Task.FromResult(true);
        }

        public Task<ReceiveMessageResponse> ReceiveMessageAsync(string queueUrl, int? maxNumberOfMessages = null, int? visibilityTimeout = null,
            int? waitTimeSeconds = null)
        {
            Queue.Enqueue(CreateQueueMessage());
            return Task.FromResult(new ReceiveMessageResponse
            {
                Messages = new List<Message>
                {
                    new Message
                    {
                        Body = Queue.Dequeue(),
                        ReceiptHandle = ""
                    }
                }
            });
        }

        private static string CreateQueueMessage()
        {
            return "{\"guid\":\"7766F2D9-38B0-4262-9CD9-E45298B52B09\",\"adn\":\"123\",\"destinations\":[{\"id\":1,\"name\":\"dest1\"},{\"id\":2,\"name\":\"dest2\"},{\"id\":3,\"name\":\"dest3\"}]}";
        }

        public Task<bool> DeleteMessage(string messageId, string receiptHandle, string queueUrl)
        {
            if (Queue.Count > 0)
                Queue.Dequeue();

            return Task.FromResult(Queue.Count == 0);
        }

        public Task<object[]> GetAggregatedMessages(List<Message> messages)
        {
            return Task.FromResult(new object[]
            {

            });
        }

        public Task<bool> DeleteBatchMessages(List<DeleteMessageBatchRequestEntry> entries, string queueUrl)
        {
            return Task.FromResult(true);
        }
    }
}
