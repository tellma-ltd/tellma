using Tellma.Model.Admin;

namespace Tellma.Client
{
    public class IdentityServerClientsClient : CrudClientBase<IdentityServerClientForSave, IdentityServerClient, int>
    {
        internal IdentityServerClientsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "identity-server-clients";
    }
}
