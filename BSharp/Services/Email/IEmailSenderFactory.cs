namespace BSharp.Services.Email
{
    public interface IEmailSenderFactory
    {
        IEmailSender Create();
    }
}