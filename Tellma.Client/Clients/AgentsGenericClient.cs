using Tellma.Model.Application;

namespace Tellma.Client
{
    public class AgentsGenericClient : FactWithIdClientBase<Agent, int>
    {
        internal AgentsGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "agents";
    }
}
