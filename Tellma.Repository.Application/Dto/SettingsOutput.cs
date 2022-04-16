using System;
using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    public class SettingsOutput
    {
        public SettingsOutput(Guid version, int? singleBusinessUnitId, GeneralSettings gSettings, FinancialSettings fSettings, IDictionary<string, bool> featureFlags)
        {
            Version = version;
            SingleBusinessUnitId = singleBusinessUnitId;
            GeneralSettings = gSettings;
            FinancialSettings = fSettings;
            FeatureFlags = featureFlags;
        }

        public Guid Version { get; }
        public int? SingleBusinessUnitId { get; } // Set when there is exactly one business unitId
        public GeneralSettings GeneralSettings { get; }
        public FinancialSettings FinancialSettings { get; }

        public IDictionary<string, bool> FeatureFlags { get; }
    }
}
