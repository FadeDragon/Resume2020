namespace EmailService.Core.Models
{
    public enum SendCode
    {
        None = 0,
        To = 1,
        CC = 2,
        BCC = 3
    }

    public class Recipient
    {
        public string Email { get; set; }

        public string Name { get; set; }

        public string Language { get; set; }

        public SendCode SendCode { get; set; }
    }
}
