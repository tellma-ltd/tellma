using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "AccountType", Plural = "AccountTypes")]
    public class AccountTypeForSave<TCustodyDef, TResourceDef> : EntityWithKey<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [AlwaysAccessible]
        [SelfReferencing(nameof(ParentIndex))]
        public int? ParentId { get; set; }

        [Display(Name = "Code")]
        [Required]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "AccountType_Concept")]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Concept { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description2 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description3 { get; set; }

        [Display(Name = "AccountType_IsMonetary")]
        [AlwaysAccessible]
        public bool? IsMonetary { get; set; }

        [Display(Name = "IsAssignable")]
        [AlwaysAccessible]
        public bool? IsAssignable { get; set; }

        [Display(Name = "AccountType_StandardAndPure")]
        [AlwaysAccessible]
        public bool? StandardAndPure { get; set; }

        [Display(Name = "AccountType_CustodianDefinition")]
        [AlwaysAccessible]
        public int? CustodianDefinitionId { get; set; }

        [Display(Name = "AccountType_ParticipantDefinition")]
        [AlwaysAccessible]
        public int? ParticipantDefinitionId { get; set; }

        [Display(Name = "AccountType_EntryTypeParent")]
        [AlwaysAccessible]
        public int? EntryTypeParentId { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time1Label", Language = Language.Primary)]
        [StringLength(50)]
        public string Time1Label { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time1Label", Language = Language.Secondary)]
        [StringLength(50)]
        public string Time1Label2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time1Label", Language = Language.Ternary)]
        [StringLength(50)]
        public string Time1Label3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time2Label", Language = Language.Primary)]
        [StringLength(50)]
        public string Time2Label { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time2Label", Language = Language.Secondary)]
        [StringLength(50)]
        public string Time2Label2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_Time2Label", Language = Language.Ternary)]
        [StringLength(50)]
        public string Time2Label3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_ExternalReferenceLabel", Language = Language.Primary)]
        [StringLength(50)]
        public string ExternalReferenceLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_ExternalReferenceLabel", Language = Language.Secondary)]
        [StringLength(50)]
        public string ExternalReferenceLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_ExternalReferenceLabel", Language = Language.Ternary)]
        [StringLength(50)]
        public string ExternalReferenceLabel3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_InternalReferenceLabel", Language = Language.Primary)]
        [StringLength(50)]
        public string InternalReferenceLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_InternalReferenceLabel", Language = Language.Secondary)]
        [StringLength(50)]
        public string InternalReferenceLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_InternalReferenceLabel", Language = Language.Ternary)]
        [StringLength(50)]
        public string InternalReferenceLabel3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAgentNameLabel", Language = Language.Primary)]
        [StringLength(50)]
        public string NotedAgentNameLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAgentNameLabel", Language = Language.Secondary)]
        [StringLength(50)]
        public string NotedAgentNameLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAgentNameLabel", Language = Language.Ternary)]
        [StringLength(50)]
        public string NotedAgentNameLabel3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAmountLabel", Language = Language.Primary)]
        [StringLength(50)]
        public string NotedAmountLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAmountLabel", Language = Language.Secondary)]
        [StringLength(50)]
        public string NotedAmountLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedAmountLabel", Language = Language.Ternary)]
        [StringLength(50)]
        public string NotedAmountLabel3 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedDateLabel", Language = Language.Primary)]
        [StringLength(50)]
        public string NotedDateLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedDateLabel", Language = Language.Secondary)]
        [StringLength(50)]
        public string NotedDateLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_NotedDateLabel", Language = Language.Ternary)]
        [StringLength(50)]
        public string NotedDateLabel3 { get; set; }

        [Display(Name = "AccountType_CustodyDefinitions")]
        [ForeignKey(nameof(AccountTypeCustodyDefinition.AccountTypeId))]
        public List<TCustodyDef> CustodyDefinitions { get; set; }

        [Display(Name = "AccountType_ResourceDefinitions")]
        [ForeignKey(nameof(AccountTypeResourceDefinition.AccountTypeId))]
        public List<TResourceDef> ResourceDefinitions { get; set; }
    }

    public class AccountTypeForSave : AccountTypeForSave<AccountTypeCustodyDefinitionForSave, AccountTypeResourceDefinitionForSave>
    {
    }

    public class AccountType : AccountTypeForSave<AccountTypeCustodyDefinition, AccountTypeResourceDefinition>
    {
        [AlwaysAccessible]
        public string Path { get; set; }

        [AlwaysAccessible]
        public short? Level { get; set; }

        [AlwaysAccessible]
        public int? ActiveChildCount { get; set; }

        [AlwaysAccessible]
        public int? ChildCount { get; set; }

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        [AlwaysAccessible]
        public bool? IsBusinessUnit { get; set; }

        [Display(Name = "IsSystem")]
        [AlwaysAccessible]
        public bool? IsSystem { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [AlwaysAccessible]
        public HierarchyId Node { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public AccountType Parent { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }

        [Display(Name = "AccountType_CustodianDefinition")]
        [ForeignKey(nameof(CustodianDefinitionId))]
        public RelationDefinition CustodianDefinition { get; set; }

        [Display(Name = "AccountType_ParticipantDefinition")]
        [ForeignKey(nameof(ParticipantDefinitionId))]
        public RelationDefinition ParticipantDefinition { get; set; }

        [Display(Name = "AccountType_EntryTypeParent")]
        [ForeignKey(nameof(EntryTypeParentId))]
        public EntryType EntryTypeParent { get; set; }
    }
}
