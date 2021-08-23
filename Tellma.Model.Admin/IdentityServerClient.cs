using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Admin
{
    [Display(Name = "IdentityServerClient", GroupName = "IdentityServerClients")]
    public class IdentityServerClientForSave : EntityWithKey<int>
    {
        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Memo")]
        [StringLength(1024)]
        public string Memo { get; set; }

        [Display(Name = "IdentityServerClient_ClientId")]
        [Required]
        public string ClientId { get; set; }

        [Display(Name = "IdentityServerClient_ClientSecret")]
        [Required]
        public string ClientSecret { get; set; }
    }

    public class IdentityServerClient : IdentityServerClientForSave
    {
        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public AdminUser CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public AdminUser ModifiedBy { get; set; }
    }
}
