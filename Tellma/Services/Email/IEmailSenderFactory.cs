namespace Tellma.Services.Email
{
    public interface IEmailSenderFactory
    {
        IEmailSender Create();
    }
}