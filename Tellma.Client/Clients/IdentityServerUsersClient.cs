using Tellma.Model.Admin;

namespace Tellma.Client
{
    public class IdentityServerUsersClient : FactGetByIdClientBase<IdentityServerUser, string>
    {
        internal IdentityServerUsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "identity-server-users";
    }
}
