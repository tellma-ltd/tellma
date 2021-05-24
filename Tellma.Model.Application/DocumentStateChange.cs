using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "DocumentStateChange", GroupName = "DocumentStateChanges")]
    public class DocumentStateChange : EntityWithKey<int>
    {
        [Required]
        public int? DocumentId { get; set; }

        [Display(Name = "StateHistory_FromState")]
        [Required]
        public short? FromState { get; set; }

        [Display(Name = "StateHistory_ToState")]
        [Required]
        public short? ToState { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
