using System.Collections.Generic;
using System.Threading.Tasks;

namespace BSharp.Data
{
    /// <summary>
    /// Extension methods for <see cref="ApplicationRepository"/> and <see cref="AdminRepository"/>
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Just makes it easier to call <see cref="ApplicationRepository.Action_Views__Permissions(string, IEnumerable{string})"/> where there is a single view Id
        /// </summary>
        public static async Task<IEnumerable<AbstractPermission>> GetUserPermissions(this ApplicationRepository repo, string action, params string[] viewIds)
        {
            return await repo.Action_Views__Permissions(action, viewIds);
        }
    }
}
