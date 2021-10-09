using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    public class PermissionsForClient
    {
        public IEnumerable<UserPermission> Permissions { get; set; }

        public IEnumerable<int> ReportIds { get; set; }

        public IEnumerable<int> DashboardIds { get; set; }

        public IEnumerable<int> TemplateIds { get; set; }
    }
}
