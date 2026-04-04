using Tellma.Model.Application;

namespace Tellma.Client
{
    public class MessageCommandsClient : FactGetByIdClientBase<MessageCommand, int>
    {
        internal MessageCommandsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "message-commands";
    }
}
