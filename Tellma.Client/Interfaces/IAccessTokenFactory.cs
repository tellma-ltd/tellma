using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Client
{
    public interface IAccessTokenFactory
    {
        Task<string> GetValidAccessToken(CancellationToken cancellation = default);
    }
}
