using System.Threading.Tasks;
using Tellma.Controllers.Dto;

namespace Tellma.Services.ClientApp
{
    /// <summary>
    /// This interface implemented on the client side.
    /// </summary>
    public interface INotifiedClient
    {
        Task UpdateInbox(InboxStatusToSend notification);
        Task InvalidateCache(CacheStatusToSend notification);
    }
}
