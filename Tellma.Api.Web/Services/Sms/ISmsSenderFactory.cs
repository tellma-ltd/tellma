namespace Tellma.Services.Sms
{
    public interface ISmsSenderFactory
    {
        ISmsSender Create();
    }
}
