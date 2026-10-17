using System;
using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    public class SettingsOutput(
        Guid version,
        int? singleBusinessUnitId,
        GeneralSettings gSettings,
        FinancialSettings fSettings,
        ZatcaSettings zSettings,
        MarminAeSettings mSettings,
        IDictionary<string, bool> featureFlags)
    {
        public Guid Version { get; } = version;
        public int? SingleBusinessUnitId { get; } = singleBusinessUnitId;
        public GeneralSettings GeneralSettings { get; } = gSettings;
        public FinancialSettings FinancialSettings { get; } = fSettings;
        public ZatcaSettings ZatcaSettings { get; } = zSettings;
        public MarminAeSettings MarminAeSettings { get; } = mSettings;
        public IDictionary<string, bool> FeatureFlags { get; } = featureFlags;
    }

    /// <summary>
    /// The Marmin (UAE) settings that need real Settings columns rather than the CustomFields
    /// bag: the two secrets, which must never reach the browser, and the environment, which is
    /// DBA-set rather than user-editable so a tenant cannot be flipped to Production from a form.
    /// </summary>
    public class MarminAeSettings
    {
        /// <summary>AES-encrypted. Decrypted with the key at <see cref="MarminAeEncryptionKeyIndex"/>.</summary>
        public string MarminAeEncryptedClientSecret { get; set; }

        /// <summary>
        /// AES-encrypted, and semicolon-separated so a rotation can publish "new;old" and accept
        /// deliveries signed with either until the vendor has switched over.
        /// </summary>
        public string MarminAeEncryptedWebhookSecret { get; set; }

        /// <summary>Index into the comma-separated MarminAe:EncryptionKeys app setting.</summary>
        public int MarminAeEncryptionKeyIndex { get; set; }

        /// <summary>Sandbox or Production.</summary>
        public string MarminAeEnvironment { get; set; }
    }

    public class ZatcaSettings
    {
        public string ZatcaEncryptedSecret { get; set; }
        public string ZatcaEncryptedSecurityToken { get; set; }
        public int ZatcaEncryptionKeyIndex { get; set; }
        public string ZatcaEnvironment { get; set; }
    }
}
