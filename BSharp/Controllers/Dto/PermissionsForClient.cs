using System.Collections.Generic;

namespace BSharp.Controllers.Dto
{
    /// <summary>
    /// This DTO carries permission information to the client so
    /// the client can adjust the UI accordingly, the string key
    /// in the dictionary represents the ViewId
    /// </summary>
    public class PermissionsForClient : Dictionary<string, Dictionary<string, bool>>
    {
        // ViewId -> Action -> True
    }
}
