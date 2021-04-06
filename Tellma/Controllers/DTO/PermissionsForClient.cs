using System.Collections.Generic;

namespace Tellma.Controllers.Dto
{
    public class PermissionsForClient
    {
        public PermissionsForClientViews Views { get; set; }

        public List<int> ReportIds { get; set; }

        public List<int> DashboardIds { get; set; }
    }

    /// <summary>
    /// This DTO carries permission information to the client so
    /// the client can adjust the UI accordingly, the string key
    /// in the dictionary represents the View
    /// </summary>
    public class PermissionsForClientViews : Dictionary<string, Dictionary<string, bool>>
    {
        // View -> Action -> True
    }
}
