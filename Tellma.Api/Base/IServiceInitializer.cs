using System.Threading.Tasks;

namespace Tellma.Controllers
{
    public interface IServiceInitializer
    {
        /// <summary>
        /// Called every time <see cref="ServiceBase.Initialize(ServiceContext)"/> is invoked.
        /// </summary>
        /// <returns>The current user Id.</returns>
        Task<int> OnInitialize(ServiceContext ctx);
    }
}
