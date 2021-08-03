using Tellma.Api.Dto;

namespace Tellma.Controllers.Dto
{
    public class SaveSettingsResponse<TSettings> : GetEntityResponse<TSettings>
    {
        public Versioned<SettingsForClient> SettingsForClient { get; set; }
    }
}
