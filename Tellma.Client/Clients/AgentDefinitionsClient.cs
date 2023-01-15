using Tellma.Model.Application;

namespace Tellma.Client
{
    public class AgentDefinitionsClient : CrudClientBase<AgentDefinitionForSave, AgentDefinition, int>
    {
        internal AgentDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "agent-definitions";
    }
}
