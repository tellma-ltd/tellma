using Tellma.Model.Application;

namespace Tellma.Client
{
    public class MessageTemplatesClient : CrudClientBase<MessageTemplateForSave, MessageTemplate, int>
    {
        internal MessageTemplatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "message-templates";
    }
}
