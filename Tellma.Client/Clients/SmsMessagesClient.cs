using Tellma.Model.Application;

namespace Tellma.Client
{
    public class SmsMessagesClient : FactGetByIdClientBase<MessageForQuery, int>
    {
        internal SmsMessagesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "sms-messages";
    }
}
