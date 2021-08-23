using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public static class PermissionsCacheExtensions
    {
        public static async Task<IEnumerable<AbstractPermission>> PermissionsFromCache(
            this IPermissionsCache cache, 
            int tenantId, 
            int userId, 
            string version, 
            string view, 
            string action, 
            CancellationToken cancellation)
        {
            var permissions = (await cache.GetPermissionss(userId, tenantId, version, cancellation)).Data.Permissions;

            // Apply view filter
            permissions = permissions.Where(e => e.View == view || e.View == "all");

            // Apply action filter (read action is implicity in all 
            if (action != PermissionActions.Read)
            {
                permissions = permissions.Where(e => e.Action == action || e.Action == "All");
            }

            // Return
            return permissions.Select(p => new AbstractPermission
            {
                Action = p.Action,
                Criteria = p.Criteria,
                Mask = null,
                View = p.View
            });
        }

        public static async Task<IEnumerable<AbstractPermission>> GenericPermissionsFromCache(
            this IPermissionsCache cache,
            int tenantId,
            int userId,
            string version,
            string viewPrefix, 
            string action, 
            CancellationToken cancellation)
        {
            var permissions = (await cache.GetPermissionss(userId, tenantId, version, cancellation)).Data.Permissions;

            // Apply view filter
            permissions = permissions.Where(e => e.View.StartsWith(viewPrefix) || e.View == "all");

            // Apply action filter (read action is implicity in all 
            if (action != PermissionActions.Read)
            {
                permissions = permissions.Where(e => e.Action == action || e.Action == "All");
            }

            // Return
            return permissions.Select(p => new AbstractPermission
            {
                Action = p.Action,
                Criteria = p.Criteria,
                Mask = null,
                View = p.View
            });
        }
    }
}