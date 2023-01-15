using Tellma.Model.Application;

namespace Tellma.Client
{
    public class OutboxClient : FactWithIdClientBase<OutboxRecord, int>
    {
        internal OutboxClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "outbox";
    }
}
