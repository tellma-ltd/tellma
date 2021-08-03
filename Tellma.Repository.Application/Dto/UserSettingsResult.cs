using System;
using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    public class UserSettingsResult
    {
        public UserSettingsResult(Guid version, User user, IEnumerable<(string Key, string Value)> customSettings)
        {
            Version = version;
            User = user;
            CustomSettings = customSettings;
        }

        public Guid Version { get; }
        public User User { get; }
        public IEnumerable<(string Key, string Value)> CustomSettings { get; }
    }
}
