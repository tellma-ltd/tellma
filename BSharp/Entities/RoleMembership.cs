using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    public class RoleMembershipForSave : EntityWithKey<int>
    {
        [Display(Name = "RoleMembership_User")]
        [AlwaysAccessible]
        public int? AgentId { get; set; }

        [Display(Name = "RoleMembership_Role")]
        public int? RoleId { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Memo { get; set; }
    }

    public class RoleMembership : RoleMembershipForSave
    {
        //[Display(Name = "CreatedAt")]
        //public DateTimeOffset? CreatedAt { get; set; }

        //[Display(Name = "CreatedBy")]
        //public int? CreatedById { get; set; }

        //[Display(Name = "ModifiedAt")]
        //public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }


        // For Query

        [Display(Name = "RoleMembership_User")]
        [ForeignKey(nameof(AgentId))]
        public User Agent { get; set; }

        [Display(Name = "RoleMembership_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        //[Display(Name = "CreatedBy")]
        //[ForeignKey(nameof(CreatedById))]
        //public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
