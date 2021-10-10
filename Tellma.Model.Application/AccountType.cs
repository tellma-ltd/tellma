using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "AccountType", GroupName = "AccountTypes")]
    public class AccountTypeForSave<TAgentDef, TResourceDef, TNotedAgentDef, TNotedResourceDef> : EntityWithKey<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [SelfReferencing(nameof(ParentIndex))]
        public int? ParentId { get; set; }

        [Display(Name = "Code")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string Code { get; set; }

        [Display(Name = "AccountType_Concept")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Concept { get; set; }

        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name3 { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description2 { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description3 { get; set; }

        [Display(Name = "AccountType_IsMonetary")]
        public bool? IsMonetary { get; set; }

        [Display(Name = "IsAssignable")]
        [Required]
        public bool? IsAssignable { get; set; }

        [Display(Name = "AccountType_StandardAndPure")]
        public bool? StandardAndPure { get; set; }

        [Display(Name = "AccountType_EntryTypeParent")]
        public int? EntryTypeParentId { get; set; }

        [Display(Name = "AccountType_Time1Label")]
        [StringLength(50)]
        public string Time1Label { get; set; }

        [Display(Name = "AccountType_Time1Label")]
        [StringLength(50)]
        public string Time1Label2 { get; set; }

        [Display(Name = "AccountType_Time1Label")]
        [StringLength(50)]
        public string Time1Label3 { get; set; }

        [Display(Name = "AccountType_Time2Label")]
        [StringLength(50)]
        public string Time2Label { get; set; }

        [Display(Name = "AccountType_Time2Label")]
        [StringLength(50)]
        public string Time2Label2 { get; set; }

        [Display(Name = "AccountType_Time2Label")]
        [StringLength(50)]
        public string Time2Label3 { get; set; }

        [Display(Name = "AccountType_ExternalReferenceLabel")]
        [StringLength(50)]
        public string ExternalReferenceLabel { get; set; }

        [Display(Name = "AccountType_ExternalReferenceLabel")]
        [StringLength(50)]
        public string ExternalReferenceLabel2 { get; set; }

        [Display(Name = "AccountType_ExternalReferenceLabel")]
        [StringLength(50)]
        public string ExternalReferenceLabel3 { get; set; }

        [Display(Name = "AccountType_ReferenceSourceLabel")]
        [StringLength(50)]
        public string ReferenceSourceLabel { get; set; }

        [Display(Name = "AccountType_ReferenceSourceLabel")]
        [StringLength(50)]
        public string ReferenceSourceLabel2 { get; set; }

        [Display(Name = "AccountType_ReferenceSourceLabel")]
        [StringLength(50)]
        public string ReferenceSourceLabel3 { get; set; }

        [Display(Name = "AccountType_InternalReferenceLabel")]
        [StringLength(50)]
        public string InternalReferenceLabel { get; set; }

        [Display(Name = "AccountType_InternalReferenceLabel")]
        [StringLength(50)]
        public string InternalReferenceLabel2 { get; set; }

        [Display(Name = "AccountType_InternalReferenceLabel")]
        [StringLength(50)]
        public string InternalReferenceLabel3 { get; set; }

        [Display(Name = "AccountType_NotedAgentNameLabel")]
        [StringLength(50)]
        public string NotedAgentNameLabel { get; set; }

        [Display(Name = "AccountType_NotedAgentNameLabel")]
        [StringLength(50)]
        public string NotedAgentNameLabel2 { get; set; }

        [Display(Name = "AccountType_NotedAgentNameLabel")]
        [StringLength(50)]
        public string NotedAgentNameLabel3 { get; set; }

        [Display(Name = "AccountType_NotedAmountLabel")]
        [StringLength(50)]
        public string NotedAmountLabel { get; set; }

        [Display(Name = "AccountType_NotedAmountLabel")]
        [StringLength(50)]
        public string NotedAmountLabel2 { get; set; }

        [Display(Name = "AccountType_NotedAmountLabel")]
        [StringLength(50)]
        public string NotedAmountLabel3 { get; set; }

        [Display(Name = "AccountType_NotedDateLabel")]
        [StringLength(50)]
        public string NotedDateLabel { get; set; }

        [Display(Name = "AccountType_NotedDateLabel")]
        [StringLength(50)]
        public string NotedDateLabel2 { get; set; }

        [Display(Name = "AccountType_NotedDateLabel")]
        [StringLength(50)]
        public string NotedDateLabel3 { get; set; }

        [Display(Name = "AccountType_AgentDefinitions")]
        [ForeignKey(nameof(AccountTypeAgentDefinition.AccountTypeId))]
        public List<TAgentDef> AgentDefinitions { get; set; }

        [Display(Name = "AccountType_ResourceDefinitions")]
        [ForeignKey(nameof(AccountTypeResourceDefinition.AccountTypeId))]
        public List<TResourceDef> ResourceDefinitions { get; set; }

        [Display(Name = "AccountType_NotedAgentDefinitions")]
        [ForeignKey(nameof(AccountTypeNotedAgentDefinition.AccountTypeId))]
        public List<TNotedAgentDef> NotedAgentDefinitions { get; set; }

        [Display(Name = "AccountType_NotedResourceDefinitions")]
        [ForeignKey(nameof(AccountTypeNotedResourceDefinition.AccountTypeId))]
        public List<TNotedResourceDef> NotedResourceDefinitions { get; set; }
    }

    public class AccountTypeForSave : AccountTypeForSave<AccountTypeAgentDefinitionForSave, AccountTypeResourceDefinitionForSave, AccountTypeNotedAgentDefinitionForSave, AccountTypeNotedResourceDefinitionForSave>
    {
    }

    public class AccountType : AccountTypeForSave<AccountTypeAgentDefinition, AccountTypeResourceDefinition, AccountTypeNotedAgentDefinition, AccountTypeNotedResourceDefinition>
    {
        public string Path { get; set; }

        [Required]
        public short? Level { get; set; }

        [Required]
        public int? ActiveChildCount { get; set; }

        [Required]
        public int? ChildCount { get; set; }

        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }

        [Display(Name = "IsSystem")]
        [Required]
        public bool? IsSystem { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? SavedAt { get; set; }

        // For Query

        [Required]
        public HierarchyId Node { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public AccountType Parent { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }

        [Display(Name = "AccountType_EntryTypeParent")]
        [ForeignKey(nameof(EntryTypeParentId))]
        public EntryType EntryTypeParent { get; set; }
    }
}
