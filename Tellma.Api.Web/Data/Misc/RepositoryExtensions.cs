using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Data
{
    // TODO: Remove

    /// <summary>
    /// Extension methods for <see cref="ApplicationRepository"/> and <see cref="AdminRepository"/>
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Syntactic sugar for <see cref="AdminRepository.Action_View__Permissions(string, string)"/>
        /// </summary>
        public static async Task<IEnumerable<AbstractPermission>> UserPermissions(this AdminRepository repo, string action, string view, CancellationToken cancellation)
        {
            return await repo.Action_View__Permissions(action, view, cancellation);
        }
    }
}
