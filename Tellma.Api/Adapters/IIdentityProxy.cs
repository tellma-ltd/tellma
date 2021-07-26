using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Api
{    
    /// <summary>
     /// The implementation allows the API to: <br/>
     /// 1 - Create users in the identity server configured as authority in this installation. <br/>
     /// 2 - Send invitation emails from a tenant to new users. <br/>
     /// </summary>
     /// <remarks>If no implementation is supplied it is assumed that the identity server is external and inaccessible.</remarks>
    public interface IIdentityProxy
    {
        /// <summary>
        /// Whether or not implementations can create users in the identity server.
        /// </summary>
        public bool CanCreateUsers { get; }

        /// <summary>
        /// For each email in <paramref name="emails"/> create a user in the identity server
        /// with that email if that user does not exist.
        /// </summary>
        /// <param name="emails">The collection of emails to create as users.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task CreateUsersIfNotExist(IEnumerable<string> emails);

        /// <summary>
        /// Whether or not implementations can send email invitations to users.
        /// </summary>
        public bool CanInviteUsers { get; }

        /// <summary>
        /// Send invitation emails to the <paramref name="users"/> to join the company with Id <paramref name="tenantId"/>.
        /// </summary>
        /// <param name="tenantId">The id of the inviting company.</param>
        /// <param name="users">The invited users.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InviteUsersToTenant(int tenantId, IEnumerable<UserForInvitation> users);
    }
}
