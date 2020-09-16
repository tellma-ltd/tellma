using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionColumn", Plural = "LineDefinitionColumns")]
    public class LineDefinitionColumnForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionColumn_ColumnName")]
        [ChoiceList(new object[] { "Memo", "PostingDate", "TemplateLineId",
            "Multiplier", "AccountId", "CurrencyId",
            "CustodianId", "CustodyId", "ParticipantId","ResourceId",  "CenterId", "EntryTypeId",
            "MonetaryValue", "Quantity", "UnitId", "Time1", "Time2", "Value",
            "ExternalReference", "AdditionalReference", "NotedRelationId",
            "NotedAgentName", "NotedAmount", "NotedDate" }, 
            new string[] { "Memo", "Line_PostingDate", "Line_TemplateLine",
            "Line_Multiplier", "Entry_Account", "Entry_Currency",
            "Entry_Custodian", "Entry_Custody", "Entry_Participant", "Entry_Resource", "Entry_Center", "Entry_EntryType",
            "Entry_MonetaryValue", "Entry_Quantity", "Entry_Unit", "Entry_Time1", "Entry_Time2", "Entry_Value",
            "Entry_ExternalReference", "Entry_AdditionalReference", "Entry_NotedRelation",
            "Entry_NotedAgentName", "Entry_NotedAmount", "Entry_NotedDate" })]
        [Required]
        public string ColumnName { get; set; }

        [Display(Name = "LineDefinitionColumn_EntryIndex")]
        [Required]
        public int? EntryIndex { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(50)]
        [AlwaysAccessible]
        [Required]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "LineDefinitionColumn_Filter")]
        public string Filter { get; set; }

        [Display(Name = "LineDefinitionColumn_InheritsFromHeader")]
        [ChoiceList(new object[] { InheritsFrom.None, InheritsFrom.TabHeader, InheritsFrom.DocumentHeader },
            new string[] { "InheritsFrom_0", "InheritsFrom_1", "InheritsFrom_2" })]
        public byte? InheritsFromHeader { get; set; }

        [Display(Name = "LineDefinitionColumn_VisibleState")]
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
        [AlwaysAccessible]
        public int? Index { get; set; }

        [Display(Name = "LineDefinitionColumn_LineDefinition")]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }

    public static class InheritsFrom
    {
        public const byte None = 0;
        public const byte TabHeader = 0;
        public const byte DocumentHeader = 0;
    }
}
