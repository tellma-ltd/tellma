using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.MultiTenancy
{
    /// <summary>
    /// Encapsulates all information that is efficiently loaded from <see cref="Data.ApplicationContext"/>
    /// as soon as we open a connection to it, this object ends up stored in the HTTP Context
    /// and made available to all the C# logic, accessible through <see cref="ITenantUserInfoAccessor"/>
    /// </summary>
    public class TenantUserInfo
    {
        // User Info
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Email { get; set; }
        public string ExternalId { get; set; }
        public string PermissionsVersion { get; set; }
        public string UserSettingsVersion { get; set; }

        // Tenant Info
        public string PrimaryLanguageId { get; set; }
        public string PrimaryLanguageSymbol { get; set; }
        public string SecondaryLanguageId { get; set; }
        public string SecondaryLanguageSymbol { get; set; }
        public string TernaryLanguageId { get; set; }
        public string TernaryLanguageSymbol { get; set; }
        public string SettingsVersion { get; set; }
        public string ViewsAndSpecsVersion { get; set; }
    }
}
