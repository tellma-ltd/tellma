using Tellma.Model.Application;

namespace Tellma.Client
{
    public class IfrsConceptsClient : FactGetByIdClientBase<IfrsConcept, int>
    {
        internal IfrsConceptsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "ifrs-concepts";
    }
}
