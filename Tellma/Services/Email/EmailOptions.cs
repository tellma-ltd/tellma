namespace Tellma.Services.Email
{
    public class EmailOptions
    {
        public SendGridOptions SendGrid { get; set; } = new SendGridOptions();
    }
}