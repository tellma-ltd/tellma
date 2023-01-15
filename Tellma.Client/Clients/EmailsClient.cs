using Tellma.Model.Application;

namespace Tellma.Client
{
    public class EmailsClient : FactGetByIdClientBase<EmailForQuery, int>
    {
        internal EmailsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "emails";
    }
}
