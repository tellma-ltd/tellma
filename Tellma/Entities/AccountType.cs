using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    public class AccountTypeForSave : EntityWithKey<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [AlwaysAccessible]
        public int? ParentId { get; set; }

        [Display(Name = "AccountType_IfrsConcept")]
        [AlwaysAccessible]
        public int? IfrsConceptId { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Description { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Description2 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Description3 { get; set; }

        [Display(Name = "Code")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "IsAssignable")]
        [AlwaysAccessible]
        public bool? IsAssignable { get; set; }

        [Display(Name = "AccountType_CurrencyAssignment")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [Assignment(AssignmentType.Required)]
        public char? CurrencyAssignment { get; set; }

        [Display(Name = "AccountType_AgentAssignment")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [Assignment]
        public char? AgentAssignment { get; set; }

        [Display(Name = "AccountType_AgentDefinition")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string AgentDefinitionId { get; set; }

        [Display(Name = "AccountType_ResourceAssignment")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [Assignment]
        public char? ResourceAssignment { get; set; }

        [Display(Name = "AccountType_ResourceDefinition")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string ResourceDefinitionId { get; set; }

        [Display(Name = "AccountType_CenterAssignment")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [Assignment(AssignmentType.Required)]
        public char? CenterAssignment { get; set; }

        [Display(Name = "AccountType_EntryTypeAssignment")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [Assignment]
        public char? EntryTypeAssignment { get; set; }

        [Display(Name = "AccountType_EntryTypeParent")]
        [AlwaysAccessible]
        public int? EntryTypeParentId { get; set; } // Only if EntryTypeAssignment <> 'N'

        [Display(Name = "AccountType_IdentifierAssignment")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [Assignment]
        public char? IdentifierAssignment { get; set; }

        [MultilingualDisplay(Name = "AccountType_IdentifierLabel", Language = Language.Primary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string IdentifierLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_IdentifierLabel", Language = Language.Secondary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string IdentifierLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_IdentifierLabel", Language = Language.Ternary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string IdentifierLabel3 { get; set; }

        [Display(Name = "AccountType_NotedAgentAssignment")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [Assignment(AssignmentType.EntryOnly)]
        public char? NotedAgentAssignment { get; set; }

        [Display(Name = "AccountType_NotedAgentDefinition")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedAgentDefinitionId { get; set; }

        [MultilingualDisplay(Name = "AccountType_DueDateLabel", Language = Language.Primary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string DueDateLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_DueDateLabel", Language = Language.Secondary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string DueDateLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_DueDateLabel", Language = Language.Ternary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string DueDateLabel3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time1Label", Language = Language.Primary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string Time1Label { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time1Label", Language = Language.Secondary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string Time1Label2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time1Label", Language = Language.Ternary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string Time1Label3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time2Label", Language = Language.Primary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string Time2Label { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time2Label", Language = Language.Secondary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string Time2Label2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time2Label", Language = Language.Ternary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string Time2Label3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_ExternalReferenceLabel", Language = Language.Primary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string ExternalReferenceLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_ExternalReferenceLabel", Language = Language.Secondary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string ExternalReferenceLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_ExternalReferenceLabel", Language = Language.Ternary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string ExternalReferenceLabel3 { get; set; }
               
        [MultilingualDisplay(Name = "AccountType_AdditionalReferenceLabel", Language = Language.Primary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string AdditionalReferenceLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_AdditionalReferenceLabel", Language = Language.Secondary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string AdditionalReferenceLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_AdditionalReferenceLabel", Language = Language.Ternary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string AdditionalReferenceLabel3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAgentNameLabel", Language = Language.Primary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedAgentNameLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAgentNameLabel", Language = Language.Secondary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedAgentNameLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAgentNameLabel", Language = Language.Ternary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedAgentNameLabel3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAmountLabel", Language = Language.Primary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedAmountLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAmountLabel", Language = Language.Secondary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedAmountLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAmountLabel", Language = Language.Ternary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedAmountLabel3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedDateLabel", Language = Language.Primary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedDateLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedDateLabel", Language = Language.Secondary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedDateLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedDateLabel", Language = Language.Ternary)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedDateLabel3 { get; set; }
    }

    public class AccountType : AccountTypeForSave
    {
        [AlwaysAccessible]
        public string Path { get; set; }

        [AlwaysAccessible]
        public short? Level { get; set; }

        [AlwaysAccessible]
        public int? ActiveChildCount { get; set; }

        [AlwaysAccessible]
        public int? ChildCount { get; set; }

        [Display(Name = "AccountType_IsResourceClassification")]
        [AlwaysAccessible]
        public bool? IsResourceClassification { get; set; }

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        [Display(Name = "IsSystem")]
        [AlwaysAccessible]
        public bool? IsSystem { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [AlwaysAccessible]
        public HierarchyId Node { get; set; }

        [AlwaysAccessible]
        public HierarchyId ParentNode { get; set; }

        [Display(Name = "AccountType_IfrsConcept")]
        [ForeignKey(nameof(IfrsConceptId))]
        public IfrsConcept IfrsConcept { get; set; }

        [Display(Name = "AccountType_AgentDefinition")]
        [ForeignKey(nameof(AgentDefinitionId))]
        public AgentDefinition AgentDefinition { get; set; }

        [Display(Name = "AccountType_ResourceDefinition")]
        [ForeignKey(nameof(ResourceDefinitionId))]
        public ResourceDefinition ResourceDefinition { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public AccountType Parent { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        [Display(Name = "AccountType_EntryTypeParent")]
        [ForeignKey(nameof(EntryTypeParentId))]
        public EntryType EntryTypeParent { get; set; }
    }
}
