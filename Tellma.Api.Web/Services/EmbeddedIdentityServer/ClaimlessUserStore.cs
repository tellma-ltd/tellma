using Tellma.Services.EmbeddedIdentityServer;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Threading;

namespace Tellma.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Overrides the default <see cref="UserOnlyStore{TUser}"/> but switches off Claims-related
    /// functionality as an optimization to reduce the number of database hits, since claims are
    /// not used
    /// </summary>
    public class ClaimlessUserStore : UserOnlyStore<EmbeddedIdentityServerUser, EmbeddedIdentityServerContext>
    {
        public const string ClaimsUnsupportedMessage = "Claims are not supported in the embedded IdentityServer instance";

        public ClaimlessUserStore(EmbeddedIdentityServerContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
        }

        public override Task AddClaimsAsync(EmbeddedIdentityServerUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(ClaimsUnsupportedMessage);
        }

        protected override IdentityUserClaim<string> CreateUserClaim(EmbeddedIdentityServerUser user, Claim claim)
        {
            throw new NotImplementedException(ClaimsUnsupportedMessage);
        }

        public override Task<IList<Claim>> GetClaimsAsync(EmbeddedIdentityServerUser user, CancellationToken cancellationToken = default)
        {
            IList<Claim> result = new List<Claim>();
            return Task.FromResult(result);
        }

        public override Task<IList<EmbeddedIdentityServerUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
            IList<EmbeddedIdentityServerUser> result = new List<EmbeddedIdentityServerUser>();
            return Task.FromResult(result);
        }

        public override Task RemoveClaimsAsync(EmbeddedIdentityServerUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(ClaimsUnsupportedMessage);
        }

        public override Task ReplaceClaimAsync(EmbeddedIdentityServerUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(ClaimsUnsupportedMessage);
        }
    }
}
