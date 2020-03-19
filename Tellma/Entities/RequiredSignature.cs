using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    public class RequiredSignature : Entity
    {
        public int LineId { get; set; }
        public int ToState { get; set; }
        public string RuleType { get; set; }

        [Display(Name = "Signature_Role")]
        public int? RoleId { get; set; }
        public int? AgentId { get; set; }
        public int? UserId { get; set; }
        public int? SignedById { get; set; }

        [Display(Name = "Signature_SignedAt")]
        public DateTimeOffset? SignedAt { get; set; }

        [Display(Name = "Signature_OnBehalfOfUser")]
        public int? OnBehalfOfUserId { get; set; }
        public bool CanSign { get; set; }
        public int? ProxyRoleId { get; set; }
        public bool CanSignOnBehalf { get; set; }

        [Display(Name = "Signature_Reason")]
        public int? ReasonId { get; set; }

        [Display(Name = "Signature_ReasonDetails")]
        public string ReasonDetails { get; set; }

        // For Query

        [Display(Name = "Signature_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(SignedById))]
        public User SignedBy { get; set; }

        [Display(Name = "Signature_OnBehalfOfUser")]
        [ForeignKey(nameof(OnBehalfOfUserId))]
        public User OnBehalfOfUser { get; set; }

        [ForeignKey(nameof(ProxyRoleId))]
        public Role ProxyRole { get; set; }

        //[Display(Name = "Signature_Reason")]
        //[ForeignKey(nameof(ReasonId))]
        //public Reason Reason { get; set; }
    }
}
