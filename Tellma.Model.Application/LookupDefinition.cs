using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LookupDefinition", GroupName = "LookupDefinitions")]
    public class LookupDefinitionForSave<TReportDefinition> : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string Code { get; set; }

        [Display(Name = "TitleSingular")]
        [Required]
        [StringLength(50)]
        public string TitleSingular { get; set; }

        [Display(Name = "TitleSingular")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string TitleSingular2 { get; set; }

        [Display(Name = "TitleSingular")]
        [StringLength(50)]
        public string TitleSingular3 { get; set; }

        [Display(Name = "TitlePlural")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string TitlePlural { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(50)]
        public string TitlePlural2 { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(50)]
        public string TitlePlural3 { get; set; }

        [Display(Name = "MainMenuIcon")]
        [StringLength(50)]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSection")]
        [StringLength(50)]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuSortKey")]
        public decimal? MainMenuSortKey { get; set; }

        [Display(Name = "Definition_ReportDefinitions")]
        [ForeignKey(nameof(LookupDefinitionReportDefinition.LookupDefinitionId))]
        public List<TReportDefinition> ReportDefinitions { get; set; }
    }

    public class LookupDefinitionForSave : LookupDefinitionForSave<LookupDefinitionReportDefinitionForSave>
    {
    }

    public class LookupDefinition : LookupDefinitionForSave<LookupDefinitionReportDefinition>
    {
        [Display(Name = "Definition_State")]
        [Required]
        [ChoiceList(new object[] {
                DefStates.Hidden,
                DefStates.Visible,
                DefStates.Archived },
            new string[] {
                DefStateNames.Hidden,
                DefStateNames.Visible,
                DefStateNames.Archived })]
        public string State { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ValidFrom { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
