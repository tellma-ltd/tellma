using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Entities
{
    public class DocumentSignature : EntityWithKey<int>
    {
        public int? DocumentId { get; set; }

        [Display(Name = "Signature_SignedAt")]
        public DateTimeOffset? SignedAt { get; set; }

        [Display(Name = "Signature_Agent")]
        public int? OnBehalfOfUserId { get; set; }

        [Display(Name = "Signature_Role")]
        public int? RoleId { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        // For Query

        [Display(Name = "Signature_OnBehalfOfUser")]
        [ForeignKey(nameof(OnBehalfOfUserId))]
        public Agent Agent { get; set; }

        [Display(Name = "Signature_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
    }
}
