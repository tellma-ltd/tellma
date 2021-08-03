using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// Represents all user settings in a particular tenant
    /// </summary>
    public class AdminUserSettingsForClient
    {
        public int? UserId { get; set; }

        public string Name { get; set; }

        public Dictionary<string, string> CustomSettings { get; set; } = new Dictionary<string, string>();
    }
}
