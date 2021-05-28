using System.Threading.Tasks;

namespace Tellma.Controllers
{
    public class NullServiceInitializer : IServiceInitializer
    {
        public Task<int> OnInitialize(ServiceContext ctx)
        {
            return Task.FromResult(0);
        }
    }
}
