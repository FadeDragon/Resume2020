namespace Diagnostics.Jobs.SlackMessageSender
{
    public interface ISlackMessageSender
    {
        void Send(string message);
    }
}
