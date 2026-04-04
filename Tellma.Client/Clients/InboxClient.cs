using Tellma.Model.Application;

namespace Tellma.Client
{
    public class InboxClient : FactWithIdClientBase<InboxRecord, int>
    {
        internal InboxClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "inbox";
    }
}
