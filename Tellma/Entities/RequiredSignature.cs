using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    public class RequiredSignature : Entity
    {
        public int LineId { get; set; }
        public int ToState { get; set; }
        public int? RoleId { get; set; }
        public int? SignedById { get; set; }
        public DateTimeOffset? SignedAt { get; set; }
        public int? OnBehalfOfUserId { get; set; }
        public bool CanSign { get; set; }
        public int? ProxyRoleId { get; set; }
        public bool CanSignOnBehalf { get; set; }

        // For Query
        // Role,SignedBy,OnBehalfOfUser,ProxyRole

        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        [ForeignKey(nameof(SignedById))]
        public User SignedBy { get; set; }

        [ForeignKey(nameof(OnBehalfOfUserId))]
        public User OnBehalfOfUser { get; set; }

        [ForeignKey(nameof(ProxyRoleId))]
        public Role ProxyRole { get; set; }
    }
}
