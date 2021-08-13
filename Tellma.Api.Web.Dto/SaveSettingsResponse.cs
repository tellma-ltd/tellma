namespace Tellma.Api.Dto
{
    public class SaveSettingsResponse<TSettings> : GetEntityResponse<TSettings>
    {
        public Versioned<SettingsForClient> SettingsForClient { get; set; }
    }
}
