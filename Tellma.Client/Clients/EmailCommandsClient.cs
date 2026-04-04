using Tellma.Model.Application;

namespace Tellma.Client
{
    public class EmailCommandsClient : FactGetByIdClientBase<EmailCommand, int>
    {
        internal EmailCommandsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "email-commands";
    }
}
