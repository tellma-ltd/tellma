using Tellma.Model.Application;

namespace Tellma.Client
{
    public class EmailTemplatesClient : CrudClientBase<EmailTemplateForSave, EmailTemplate, int>
    {
        internal EmailTemplatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "email-templates";
    }
}
