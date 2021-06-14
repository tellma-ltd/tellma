using Tellma.Model.Application;

namespace Tellma.Controllers.Dto
{
    public class SaveSettingsResponse<TSettings> : GetEntityResponse<TSettings>
    {
        public Versioned<SettingsForClient> SettingsForClient { get; set; }
    }
}
