using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Api
{
    /// <summary>
    /// Default implementation of <see cref="IIdentityProxy"/> which has no access to an identity server.
    /// </summary>
    /// <remarks>If the embedded identity server is enabled, this implementation is replaced.</remarks>
    internal class NullIdentityProxy : IIdentityProxy
    {
        public bool CanCreateUsers => false;

        public bool CanInviteUsers => false;

        public Task CreateUserIfNotExist(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task CreateUsersIfNotExist(IEnumerable<string> emails, bool emailConfirmed = false)
        {
            // Bug
            throw new InvalidOperationException("Attempt to create users through an identity proxy that does not support user creation.");
        }

        public Task CreateUsersIfNotExist(IEnumerable<string> emails)
        {
            throw new NotImplementedException();
        }

        public Task InviteUsersToAdmin(IEnumerable<AdminUserForInvitation> users)
        {
            // Bug
            throw new InvalidOperationException("Attempt to invite users through an identity proxy that does not support user invitation.");
        }

        public Task InviteUsersToTenant(int tenantId, IEnumerable<UserForInvitation> users)
        {
            // Bug
            throw new InvalidOperationException("Attempt to invite users through an identity proxy that does not support user invitation.");
        }
    }
}
