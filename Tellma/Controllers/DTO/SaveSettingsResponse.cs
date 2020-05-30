using Tellma.Entities;

namespace Tellma.Controllers.Dto
{
    public class SaveSettingsResponse : GetEntityResponse<Settings>
    {
        public Versioned<SettingsForClient> SettingsForClient { get; set; }
    }
}
