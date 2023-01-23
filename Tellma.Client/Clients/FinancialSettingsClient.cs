using Tellma.Model.Application;

namespace Tellma.Client
{
    public class FinancialSettingsClient : ApplicationSettingsClientBase<GeneralSettings, GeneralSettingsForSave>
    {
        protected override string ControllerPath => "financial-settings";

        public FinancialSettingsClient(IClientBehavior behavior) : base(behavior)
        {
        }
    }
}
