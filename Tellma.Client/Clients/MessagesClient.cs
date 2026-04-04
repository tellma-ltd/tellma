using Tellma.Model.Application;

namespace Tellma.Client
{
    public class MessagesClient : FactGetByIdClientBase<MessageForQuery, int>
    {
        internal MessagesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "messages";
    }
}
