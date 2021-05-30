using System.Threading.Tasks;

namespace Tellma.Api.Base
{
    public interface IServiceBehavior
    {
        /// <summary>
        /// Called every time <see cref="ServiceBase.Initialize()"/> is invoked.
        /// </summary>
        /// <returns>The current user Id.</returns>
        Task<int> OnInitialize();
    }
}
