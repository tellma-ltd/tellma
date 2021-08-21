using System;
using System.Collections.Generic;
using Tellma.Repository.Common;

namespace Tellma.Repository.Application
{
    public class PermissionsOutput
    {
        public PermissionsOutput(Guid version, IEnumerable<AbstractPermission> permissions, IEnumerable<int> reportIds, IEnumerable<int> dashboardIds)
        {
            Version = version;
            Permissions = permissions;
            ReportIds = reportIds;
            DashboardIds = dashboardIds;
        }

        public Guid Version { get; }
        public IEnumerable<AbstractPermission> Permissions { get; }
        public IEnumerable<int> ReportIds { get; }
        public IEnumerable<int> DashboardIds { get; }
    }
}
