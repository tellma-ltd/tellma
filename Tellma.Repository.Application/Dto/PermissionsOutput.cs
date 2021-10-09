using System;
using System.Collections.Generic;
using Tellma.Repository.Common;

namespace Tellma.Repository.Application
{
    public class PermissionsOutput
    {
        public PermissionsOutput(Guid version, IEnumerable<AbstractPermission> permissions, IEnumerable<int> reportIds, IEnumerable<int> dashboardIds, IEnumerable<int> templateIds)
        {
            Version = version;
            Permissions = permissions;
            ReportIds = reportIds;
            DashboardIds = dashboardIds;
            TemplateIds = templateIds;
        }

        public Guid Version { get; }
        public IEnumerable<AbstractPermission> Permissions { get; }
        public IEnumerable<int> ReportIds { get; }
        public IEnumerable<int> DashboardIds { get; }
        public IEnumerable<int> TemplateIds { get; }
    }
}
