namespace BSharp.Services.Email
{
    public class EmailConfiguration
    {
        public SendGridConfiguration SendGrid { get; set; } = new SendGridConfiguration();
    }

    public class SendGridConfiguration
    {
        public string DefaultFromEmail { get; set; }
        public string ApiKey { get; set; }
    }
}