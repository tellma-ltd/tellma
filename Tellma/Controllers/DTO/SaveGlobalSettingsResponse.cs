namespace Tellma.Controllers.Dto
{
    public class SaveGlobalSettingsResponse : GetByIdResponse<GlobalSettings>
    {
        public Versioned<GlobalSettingsForClient> SettingsForClient { get; set; }
    }
}
