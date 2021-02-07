using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AccountTypeCustodyDefinition", Plural = "AccountTypeCustodyDefinitions")]
    public class AccountTypeCustodyDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_CustodyDefinition")]
        [NotNull]
        [Required]
        public int? CustodyDefinitionId { get; set; }
    }

    public class AccountTypeCustodyDefinition : AccountTypeCustodyDefinitionForSave
    {
        [NotNull]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_CustodyDefinition")]
        [ForeignKey(nameof(CustodyDefinitionId))]
        public CustodyDefinition CustodyDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? SavedById { get; set; }

        // For query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
