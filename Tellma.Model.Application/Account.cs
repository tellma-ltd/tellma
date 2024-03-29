﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Account", GroupName = "Accounts")]
    public class AccountForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_Type")]
        [Required, ValidateRequired]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_Center")]
        public int? CenterId { get; set; }

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

        [Display(Name = "Code")]
        [StringLength(50)]
        public string Code { get; set; }

        [Display(Name = "Account_Classification")]
        public int? ClassificationId { get; set; }

        [Display(Name = "Account_AgentDefinition")]
        public int? AgentDefinitionId { get; set; }

        [Display(Name = "Account_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Account_ResourceDefinition")]
        public int? ResourceDefinitionId { get; set; }

        [Display(Name = "Account_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Account_NotedAgentDefinition")]
        public int? NotedAgentDefinitionId { get; set; }

        [Display(Name = "Account_NotedAgent")]
        public int? NotedAgentId { get; set; }

        [Display(Name = "Account_NotedResourceDefinition")]
        public int? NotedResourceDefinitionId { get; set; }

        [Display(Name = "Account_NotedResource")]
        public int? NotedResourceId { get; set; }

        [Display(Name = "Account_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [Display(Name = "Account_EntryType")]
        public int? EntryTypeId { get; set; }

        [Display(Name = "Account_IsAutoSelected")]
        [Required]
        public bool? IsAutoSelected { get; set; }
    }

    public class Account : AccountForSave
    {
        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }
        
        [NotMapped]
        public decimal? Balance { get; set; }

        // For Query

        [Display(Name = "Account_Type")]
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        [Display(Name = "Account_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Account_Classification")]
        [ForeignKey(nameof(ClassificationId))]
        public AccountClassification Classification { get; set; }

        [Display(Name = "Account_AgentDefinition")]
        [ForeignKey(nameof(AgentDefinitionId))]
        public AgentDefinition AgentDefinition { get; set; }

        [Display(Name = "Account_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Account_ResourceDefinition")]
        [ForeignKey(nameof(ResourceDefinitionId))]
        public ResourceDefinition ResourceDefinition { get; set; }

        [Display(Name = "Account_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Account_NotedAgentDefinition")]
        [ForeignKey(nameof(NotedAgentDefinitionId))]
        public AgentDefinition NotedAgentDefinition { get; set; }

        [Display(Name = "Account_NotedAgent")]
        [ForeignKey(nameof(NotedAgentId))]
        public Agent NotedAgent { get; set; }

        [Display(Name = "Account_NotedResourceDefinition")]
        [ForeignKey(nameof(NotedResourceDefinitionId))]
        public ResourceDefinition NotedResourceDefinition { get; set; }

        [Display(Name = "Account_NotedResource")]
        [ForeignKey(nameof(NotedResourceId))]
        public Resource NotedResource { get; set; }

        [Display(Name = "Account_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Account_EntryType")]
        [ForeignKey(nameof(EntryTypeId))]
        public EntryType EntryType { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
