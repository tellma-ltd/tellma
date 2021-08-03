using System;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    public class SettingsResult
    {
        public SettingsResult(Guid version, int? singleBusinessUnitId, GeneralSettings gSettings, FinancialSettings fSettings)
        {
            Version = version;
            SingleBusinessUnitId = singleBusinessUnitId;
            GeneralSettings = gSettings;
            FinancialSettings = fSettings;
        }

        public Guid Version { get; }
        public int? SingleBusinessUnitId { get; } // Set when there is exactly one business unitId
        public GeneralSettings GeneralSettings { get; }
        public FinancialSettings FinancialSettings { get; }
    }
}
