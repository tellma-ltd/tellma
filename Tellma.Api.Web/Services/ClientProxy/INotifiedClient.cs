using System.Threading.Tasks;
using Tellma.Controllers.Dto;

namespace Tellma.Services.ClientProxy
{
    /// <summary>
    /// This interface is implemented on the client app via SignalR.
    /// </summary>
    public interface INotifiedClient
    {
        Task UpdateInbox(InboxStatusToSend notification);
        Task InvalidateCache(CacheStatusToSend notification);
    }
}
