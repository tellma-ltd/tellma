using BSharp.EntityModel;

namespace BSharp.Controllers.Dto
{
    public class SaveSettingsResponse : GetEntityResponse<Settings>
    {
        public DataWithVersion<SettingsForClient> SettingsForClient { get; set; }
    }
}
