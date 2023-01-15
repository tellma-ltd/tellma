using Tellma.Model.Application;

namespace Tellma.Client
{
    public class PrintingTemplatesClient : CrudClientBase<PrintingTemplateForSave, PrintingTemplate, int>
    {
        internal PrintingTemplatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "printing-templates";
    }
}
