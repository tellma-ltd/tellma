using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    public class LineDefinitionEntryAccountTypeForSave : EntityWithKey<int>
    {
        public int? AccountTypeId { get; set; }
    }

    public class LineDefinitionEntryAccountType : LineDefinitionEntryAccountTypeForSave
    {
        public int? LineDefinitionEntryId { get; set; }

        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
