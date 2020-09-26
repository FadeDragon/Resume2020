using Amazon.Lambda.Core;

namespace EmailService.Service.Tests
{
    public class FakeLogger : ILambdaLogger
    {
        public void Log(string message)
        {
        }

        public void LogLine(string message)
        {
        }
    }
}
