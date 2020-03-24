using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Entities
{
    public class DocumentStateChange : EntityWithKey<int>
    {
        public int? DocumentId { get; set; }

        [Display(Name = "StateHistory_FromState")]
        public short? FromState { get; set; }

        [Display(Name = "StateHistory_ToState")]
        public short? ToState { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
