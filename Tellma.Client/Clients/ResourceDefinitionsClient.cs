using Tellma.Model.Application;

namespace Tellma.Client
{
    public class ResourceDefinitionsClient : CrudClientBase<ResourceDefinitionForSave, ResourceDefinition, int>
    {
        internal ResourceDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "resource-definitions";
    }
}
