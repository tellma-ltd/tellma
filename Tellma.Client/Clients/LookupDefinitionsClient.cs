using Tellma.Model.Application;

namespace Tellma.Client
{
    public class LookupDefinitionsClient : CrudClientBase<LookupDefinitionForSave, LookupDefinition, int>
    {
        internal LookupDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "lookup-definitions";
    }
}
