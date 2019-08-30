using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        public static async Task<IEnumerable<AbstractPermission>> UserPermissions(this ApplicationRepository repo, string action, params string[] viewIds)
        {
            return await repo.Action_Views__Permissions(action, viewIds);
        }

        /// <summary>
        /// Extension method that adds <see cref="DBNull.Value"/> when the supplied value
        /// is null, instead of the default behavior of not adding anything at all
        /// </summary>
        public static SqlParameter Add(this SqlParameterCollection target, string parameterName, object value)
        {
            value = value ?? DBNull.Value;
            return target.AddWithValue(parameterName, value);
        }
    }
}
