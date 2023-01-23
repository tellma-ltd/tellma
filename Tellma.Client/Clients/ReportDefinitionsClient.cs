using Tellma.Model.Application;

namespace Tellma.Client
{
    public class ReportDefinitionsClient : CrudClientBase<ReportDefinitionForSave, ReportDefinition, int>
    {
        internal ReportDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "report-definitions";
    }
}
