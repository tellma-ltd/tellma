using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "AccountType", Plural = "AccountTypes")]
    public class AccountTypeForSave : EntityWithKey<int>, ITreeEntityForSave<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [AlwaysAccessible]
        public int? ParentId { get; set; }

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

        [Display(Name = "Code")]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "IsAssignable")]
        [AlwaysAccessible]
        public bool? IsAssignable { get; set; }

        [Display(Name = "AccountType_EntryTypeParent")]
        [AlwaysAccessible]
        public int? EntryTypeParentId { get; set; }

        [MultilingualDisplay(Name = "AccountType_DueDateLabel", Language = Language.Primary)]
        [StringLength(50)]
        public string DueDateLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_DueDateLabel", Language = Language.Secondary)]
        [StringLength(50)]
        public string DueDateLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_DueDateLabel", Language = Language.Ternary)]
        [StringLength(50)]
        public string DueDateLabel3 { get; set; }

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
               
        [MultilingualDisplay(Name = "AccountType_AdditionalReferenceLabel", Language = Language.Primary)]
        [StringLength(50)]
        public string AdditionalReferenceLabel { get; set; }

        [MultilingualDisplay(Name = "AccountType_AdditionalReferenceLabel", Language = Language.Secondary)]
        [StringLength(50)]
        public string AdditionalReferenceLabel2 { get; set; }

        [MultilingualDisplay(Name = "AccountType_AdditionalReferenceLabel", Language = Language.Ternary)]
        [StringLength(50)]
        public string AdditionalReferenceLabel3 { get; set; }

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
    }

    public class AccountType : AccountTypeForSave, ITreeEntity<int>
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
