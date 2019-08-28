using System;
using System.Collections.Generic;

namespace BSharp.Data
{
    /// <summary>
    /// Carries the user settings from the <see cref="ApplicationRepository"/>
    /// </summary>
    public class UserSettings
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Name2 { get; set; }

        public string Name3 { get; set; }

        public string ImageId { get; set; }

        public Guid UserSettingsVersion { get; set; }

        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
