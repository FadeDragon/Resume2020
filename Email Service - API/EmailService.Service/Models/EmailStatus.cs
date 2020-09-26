namespace EmailService.Service.Models
{
    public enum EmailStatus
    {
        SentToProvider = 1,
        FailedToSendToProvider = 2,
        Blacklisted = 3
    }
}
