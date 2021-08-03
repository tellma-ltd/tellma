using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    public class AdminPermissionsForClient
    {
        public IEnumerable<UserPermission> Permissions { get; set; }
    }
}
