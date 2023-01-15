using Tellma.Model.Application;

namespace Tellma.Client
{
    public class DashboardDefinitionsClient : CrudClientBase<DashboardDefinitionForSave, DashboardDefinition, int>
    {
        internal DashboardDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "dashboard-definitions";
    }
}
