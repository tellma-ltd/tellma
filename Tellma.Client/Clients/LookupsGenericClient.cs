using Tellma.Model.Application;

namespace Tellma.Client
{
    public class LookupsGenericClient : FactWithIdClientBase<Lookup, int>
    {
        internal LookupsGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "lookups";
    }
}
