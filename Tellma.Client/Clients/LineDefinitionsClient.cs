using Tellma.Model.Application;

namespace Tellma.Client
{
    public class LineDefinitionsClient : CrudClientBase<LineDefinitionForSave, LineDefinition, int>
    {
        internal LineDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "line-definitions";
    }
}
