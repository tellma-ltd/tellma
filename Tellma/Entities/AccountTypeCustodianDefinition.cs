using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AccountTypeCustodianDefinition", Plural = "AccountTypeCustodianDefinitions")]
    public class AccountTypeCustodianDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_CustodianDefinition")]
        [Required]
        public int? CustodianDefinitionId { get; set; }
    }

    public class AccountTypeCustodianDefinition : AccountTypeCustodianDefinitionForSave
    {
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_CustodianDefinition")]
        [ForeignKey(nameof(CustodianDefinitionId))]
        public RelationDefinition CustodianDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
