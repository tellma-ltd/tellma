using System;

namespace BSharp.Controllers.Dto
{
    public class SettingsForClient
    {
        public string ShortCompanyName { get; set; }

        public string ShortCompanyName2 { get; set; }

        public string PrimaryLanguageId { get; set; }

        public string PrimaryLanguageName { get; set; }

        public string PrimaryLanguageSymbol { get; set; }

        public string SecondaryLanguageId { get; set; }

        public string SecondaryLanguageName { get; set; }

        public string SecondaryLanguageSymbol { get; set; }

        public string TernaryLanguageId { get; set; }

        public string TernaryLanguageName { get; set; }

        public string TernaryLanguageSymbol { get; set; }

        public string BrandColor { get; set; }
               
        public DateTimeOffset ProvisionedAt { get; set; }
    }
}
