using System.Configuration;
using System.Net.Http;

namespace Diagnostics.Jobs.SlackMessageSender
{
    public class SlackMessageSender : ISlackMessageSender
    {
        private readonly string slackWebhookUrl = ConfigurationManager.AppSettings["SlackIntegration_Webhook"];
        private readonly HttpClient httpClient;

        public SlackMessageSender(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async void Send(string message)
        {
            var jsonPayload = $"{{\"text\":\"{message}\"}}";
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            var result = await httpClient.PostAsync(slackWebhookUrl, content);

            if (!result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                throw new System.Exception($"Sending slack message to {slackWebhookUrl} failed with {response}");
            }
        }
    }
}
