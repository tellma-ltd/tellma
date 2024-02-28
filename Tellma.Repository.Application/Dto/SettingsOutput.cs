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
        IDictionary<string, bool> featureFlags)
    {
        public Guid Version { get; } = version;
        public int? SingleBusinessUnitId { get; } = singleBusinessUnitId;
        public GeneralSettings GeneralSettings { get; } = gSettings;
        public FinancialSettings FinancialSettings { get; } = fSettings;
        public ZatcaSettings ZatcaSettings { get; } = zSettings;
        public IDictionary<string, bool> FeatureFlags { get; } = featureFlags;
    }

    public class ZatcaSettings
    {
        public string ZatcaEncryptedSecret { get; set; }
        public string ZatcaEncryptedSecurityToken { get; set; }
        public int ZatcaEncryptionKeyIndex { get; set; }
        public string ZatcaEnvironment { get; set; }
    }
}
