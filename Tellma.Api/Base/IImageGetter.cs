using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Api.Base
{
    public interface IImageGetter : IServiceBase
    {
        public Task<ImageResult> GetImage(int id, CancellationToken cancellation);
    }
}
