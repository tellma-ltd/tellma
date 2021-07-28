﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "RelationDefinitionReportDefinition", GroupName = "RelationDefinitionReportDefinitions")]
    public class RelationDefinitionReportDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Definition_ReportDefinition")]
        [Required, ValidateRequired]
        public int? ReportDefinitionId { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name3 { get; set; }
    }

    public class RelationDefinitionReportDefinition : RelationDefinitionReportDefinitionForSave
    {
        [Required]
        public int? Index { get; set; }

        [Required]
        public int? RelationDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        [Display(Name = "Definition_ReportDefinition")]
        [ForeignKey(nameof(ReportDefinitionId))]
        public ReportDefinition ReportDefinition { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
