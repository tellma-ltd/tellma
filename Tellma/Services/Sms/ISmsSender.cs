using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Sms
{
    public interface ISmsSender
    {
        Task<string> SendAsync(string toPhoneNumber, string sms, CancellationToken cancellation);
    }
}
