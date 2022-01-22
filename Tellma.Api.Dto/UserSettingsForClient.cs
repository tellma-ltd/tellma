using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// Represents user settings in a particular tenant.
    /// </summary>
    public class UserSettingsForClient
    {
        public int? UserId { get; set; }

        public string ImageId { get; set; }

        public string Name { get; set; }

        public string Name2 { get; set; }

        public string Name3 { get; set; }

        public string Email { get; set; }

        public string PreferredLanguage { get; set; }

        public string PreferredCalendar { get; set; }

        public IReadOnlyDictionary<string, string> CustomSettings { get; set; } = new Dictionary<string, string>();
    }
}
