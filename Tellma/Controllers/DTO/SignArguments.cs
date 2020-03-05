using System;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// Arguments for singing a bunch of document lines
    /// </summary>
    public class SignArguments : ActionArguments
    {
        /// <summary>
        /// The desired new state target of this signature
        /// </summary>
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        public short ToState { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        public string RuleType { get; set; }

        /// <summary>
        /// The reason for the signature
        /// </summary>
        [Display(Name = "Signature_Reason")]
        public int? ReasonId { get; set; }

        /// <summary>
        /// An optional text comment
        /// </summary>
        [Display(Name = "Signature_ReasonDetails")]
        public string ReasonDetails { get; set; }

        /// <summary>
        /// If the user is signing on behalf of another user
        /// </summary>
        public int? OnBehalfOfUserId { get; set; }

        /// <summary>
        /// The role associated with the signature
        /// </summary>
        public int? RoleId { get; set; }

        /// <summary>
        /// Optional, to specify the time of the signature manually
        /// </summary>
        [Display(Name = "Signature_SignedAt")]
        public DateTimeOffset? SignedAt { get; set; }
    }
}
