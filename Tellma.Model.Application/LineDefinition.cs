using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinition", GroupName = "LineDefinitions")]
    public class LineDefinitionForSave<TEntry, TColumn, TStateReason, TGenerateParameter, TWorkflow> : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [Required, ValidateRequired]
        [StringLength(100)]
        public string Code { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description2 { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description3 { get; set; }

        [Display(Name = "TitleSingular")]
        [Required, ValidateRequired]
        [StringLength(100)]
        public string TitleSingular { get; set; }

        [Display(Name = "TitleSingular")]
        [StringLength(100)]
        public string TitleSingular2 { get; set; }

        [Display(Name = "TitleSingular")]
        [StringLength(100)]
        public string TitleSingular3 { get; set; }

        [Display(Name = "TitlePlural")]
        [Required, ValidateRequired]
        [StringLength(100)]
        public string TitlePlural { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(100)]
        public string TitlePlural2 { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(100)]
        public string TitlePlural3 { get; set; }

        [Display(Name = "LineDefinition_AllowSelectiveSigning")]
        [Required]
        public bool? AllowSelectiveSigning { get; set; }

        [Display(Name = "LineDefinition_ViewDefaultsToForm")]
        [Required]
        public bool? ViewDefaultsToForm { get; set; }

        [Display(Name = "LineDefinition_BarcodeColumnIndex")]
        public int? BarcodeColumnIndex { get; set; }

        [Display(Name = "LineDefinition_BarcodeProperty")]
        [StringLength(50)]
        public string BarcodeProperty { get; set; }

        [Display(Name = "LineDefinition_BarcodeExistingItemHandling")]
        [StringLength(50)]
        [ChoiceList(new object[] { 
                "AddNewLine", 
                "IncrementQuantity", 
                "ThrowError", 
                "DoNothing" },
            new string[] { 
                "LineDefinition_Handling_AddNewLine", 
                "LineDefinition_Handling_IncrementQuantity", 
                "LineDefinition_Handling_ThrowError", 
                "LineDefinition_Handling_DoNothing" })]
        public string BarcodeExistingItemHandling { get; set; }

        [Display(Name = "LineDefinition_BarcodeBeepsEnabled")]
        [Required]
        public bool? BarcodeBeepsEnabled { get; set; }

        [Display(Name = "LineDefinition_GenerateLabel")]
        [StringLength(50)]
        public string GenerateLabel { get; set; }

        [Display(Name = "LineDefinition_GenerateLabel")]
        [StringLength(50)]
        public string GenerateLabel2 { get; set; }

        [Display(Name = "LineDefinition_GenerateLabel")]
        [StringLength(50)]
        public string GenerateLabel3 { get; set; }

        [Display(Name = "LineDefinition_GenerateScript")]
        public string GenerateScript { get; set; }

        [Display(Name = "Definition_PreprocessScript")]
        public string PreprocessScript { get; set; }

        [Display(Name = "Definition_ValidateScript")]
        public string ValidateScript { get; set; }

        [Display(Name = "LineDefinition_Entries")]
        [ForeignKey(nameof(LineDefinitionEntry.LineDefinitionId))]
        public List<TEntry> Entries { get; set; }

        [Display(Name = "LineDefinition_Columns")]
        [ForeignKey(nameof(LineDefinitionColumn.LineDefinitionId))]
        public List<TColumn> Columns { get; set; }

        [Display(Name = "LineDefinition_StateReasons")]
        [ForeignKey(nameof(LineDefinitionStateReason.LineDefinitionId))]
        public List<TStateReason> StateReasons { get; set; }

        [Display(Name = "LineDefinition_GenerateParameters")]
        [ForeignKey(nameof(LineDefinitionGenerateParameter.LineDefinitionId))]
        public List<TGenerateParameter> GenerateParameters { get; set; }

        [Display(Name = "LineDefinition_Workflows")]
        [ForeignKey(nameof(Workflow.LineDefinitionId))]
        public List<TWorkflow> Workflows { get; set; }
    }

    public class LineDefinitionForSave : LineDefinitionForSave<LineDefinitionEntryForSave, LineDefinitionColumnForSave, LineDefinitionStateReasonForSave, LineDefinitionGenerateParameterForSave, WorkflowForSave>
    {
    }

    public class LineDefinition : LineDefinitionForSave<LineDefinitionEntry, LineDefinitionColumn, LineDefinitionStateReason, LineDefinitionGenerateParameter, Workflow>
    {
        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
