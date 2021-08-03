using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionColumn", GroupName = "LineDefinitionColumns")]
    public class LineDefinitionColumnForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionColumn_ColumnName")]
        [ChoiceList(new object[] { "Memo", "PostingDate", "Boolean1", "Decimal1", "Text1", "TemplateLineId",
            "Multiplier", "AccountId", "CurrencyId",
            "RelationId", "ResourceId", "NotedRelationId",  "CenterId", "EntryTypeId",
            "MonetaryValue", "Quantity", "UnitId", "Time1", "Duration", "DurationUnit", "Time2", "Value",
            "ExternalReference", "ReferenceSourceId", "InternalReference", "NotedAgentName", "NotedAmount", "NotedDate" }, 
            new string[] { "Memo", "Line_PostingDate", "Line_Boolean1", "Line_Decimal1", "Line_Text1", "Line_TemplateLine",
            "Line_Multiplier", "Entry_Account", "Entry_Currency",
            "Entry_Relation", "Entry_Resource", "Entry_NotedRelation", "Entry_Center", "Entry_EntryType",
            "Entry_MonetaryValue", "Entry_Quantity", "Entry_Unit", "Entry_Time1", "Entry_Duration", "Entry_NotedDuration", "Entry_Time2", "Entry_Value",
            "Entry_ExternalReference", "Entry_ReferenceSource", "Entry_InternalReference", "Entry_NotedAgentName", "Entry_NotedAmount", "Entry_NotedDate" })]
        [Required, ValidateRequired]
        public string ColumnName { get; set; }

        [Display(Name = "LineDefinitionColumn_EntryIndex")]
        [Required, ValidateRequired]
        public int? EntryIndex { get; set; }

        [Display(Name = "Label")]
        [StringLength(50)]
        [Required, ValidateRequired]
        public string Label { get; set; }

        [Display(Name = "Label")]
        [StringLength(50)]
        public string Label2 { get; set; }

        [Display(Name = "Label")]
        [StringLength(50)]
        public string Label3 { get; set; }

        [Display(Name = "LineDefinitionColumn_Filter")]
        public string Filter { get; set; }

        [Display(Name = "LineDefinitionColumn_InheritsFromHeader")]
        [Required]
        [ChoiceList(new object[] { InheritsFrom.None, InheritsFrom.TabHeader, InheritsFrom.DocumentHeader },
            new string[] { "InheritsFrom_0", "InheritsFrom_1", "InheritsFrom_2" })]
        public byte? InheritsFromHeader { get; set; }

        [Display(Name = "LineDefinitionColumn_VisibleState")]
        [Required]
        [ChoiceList(new object[] {
            LineState.Draft,
            LineState.Requested,
            LineState.Authorized,
            LineState.Completed,
            LineState.Posted,
            (short)5
        },
            new string[] {
            LineStateName.Draft,
            LineStateName.Requested,
            LineStateName.Authorized,
            LineStateName.Completed,
            LineStateName.Posted,
            "Never"
        })]
        public short? VisibleState { get; set; }

        [Display(Name = "LineDefinitionColumn_RequiredState")]
        [Required]
        [ChoiceList(new object[] {
            LineState.Draft,
            LineState.Requested,
            LineState.Authorized,
            LineState.Completed,
            LineState.Posted,
            (short)5
        },
            new string[] {
            LineStateName.Draft,
            LineStateName.Requested,
            LineStateName.Authorized,
            LineStateName.Completed,
            LineStateName.Posted,
            "Never"
        })]
        public short? RequiredState { get; set; }

        [Display(Name = "LineDefinitionColumn_ReadOnlyState")]
        [Required]
        [ChoiceList(new object[] {
            LineState.Draft,
            LineState.Requested,
            LineState.Authorized,
            LineState.Completed,
            LineState.Posted,
            (short)5
        },
            new string[] {
            LineStateName.Draft,
            LineStateName.Requested,
            LineStateName.Authorized,
            LineStateName.Completed,
            LineStateName.Posted,
            "Never"
        })]
        public short? ReadOnlyState { get; set; }
    }

    public class LineDefinitionColumn : LineDefinitionColumnForSave
    {
        [Required]
        public int? Index { get; set; }

        [Display(Name = "LineDefinitionColumn_LineDefinition")]
        [Required]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }

    public static class InheritsFrom
    {
        public const byte None = 0;
        public const byte TabHeader = 1;
        public const byte DocumentHeader = 2;
    }
}
