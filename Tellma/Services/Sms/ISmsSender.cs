using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(string toPhoneNumber, string sms, int? tenantId = null, int? notificationId = null, CancellationToken cancellation = default);
    }
}
