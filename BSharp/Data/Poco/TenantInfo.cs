namespace BSharp.Data
{
    /// <summary>
    /// A class for storing basic information about a particular tenant, information that is retrieved from the database
    /// </summary>
    public class TenantInfo
    {
        // Tenant Info
        public string ShortCompanyName { get; set; }
        public string ShortCompanyName2 { get; set; }
        public string ShortCompanyName3 { get; set; }
        public string DefinitionsVersion { get; set; }
        public string SettingsVersion { get; set; }
        public string PrimaryLanguageId { get; set; }
        public string PrimaryLanguageSymbol { get; set; }
        public string SecondaryLanguageId { get; set; }
        public string SecondaryLanguageSymbol { get; set; }
        public string TernaryLanguageId { get; set; }
        public string TernaryLanguageSymbol { get; set; }
    }
}
