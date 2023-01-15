using Tellma.Model.Application;

namespace Tellma.Client
{
    public class DetailsEntriesClient : FactWithIdClientBase<DetailsEntry, int>
    {
        internal DetailsEntriesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "details-entries";
    }
}
