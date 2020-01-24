using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Tellma.Data
{
    /// <summary>
    /// Extension methods for <see cref="ApplicationRepository"/> and <see cref="AdminRepository"/>
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Syntactic sugar for <see cref="ApplicationRepository.Action_View__Permissions(string, string)"/>
        /// </summary>
        public static async Task<IEnumerable<AbstractPermission>> UserPermissions(this ApplicationRepository repo, string action, string view)
        {
            return await repo.Action_View__Permissions(action, view);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="ApplicationRepository.Action_ViewPrefix__Permissions(string, string)"/>
        /// </summary>
        public static async Task<IEnumerable<AbstractPermission>> GenericUserPermissions(this ApplicationRepository repo, string action, string prefix)
        {
            return await repo.Action_ViewPrefix__Permissions(action, prefix);
        }

        /// <summary>
        /// Extension method that adds <see cref="DBNull.Value"/> when the supplied value
        /// is null, instead of the default behavior of not adding anything at all
        /// </summary>
        public static SqlParameter Add(this SqlParameterCollection target, string parameterName, object value)
        {
            value ??= DBNull.Value;
            return target.AddWithValue(parameterName, value);
        }
    }
}
