using Tellma.Model.Application;

namespace Tellma.Client
{
    public class ResourcesGenericClient : FactWithIdClientBase<Resource, int>
    {
        internal ResourcesGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "resources";
    }
}
