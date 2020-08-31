using System.Threading.Tasks;

namespace Tellma.Services.Sms
{
    public interface ISmsCallbackHandler
    {
        Task HandleCallback(SmsEvent smsEvent);
    }
}
