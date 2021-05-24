using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Admin
{
    [Display(Name = "SqlServer", GroupName = "SqlServers")]
    public class SqlServerForSave : EntityWithKey<int>
    {
        [Display(Name = "SqlServer_ServerName")]
        [Required]
        [StringLength(255)]
        public string ServerName { get; set; }

        [Display(Name = "SqlServer_UserName")]
        [StringLength(255)]
        public string UserName { get; set; }

        [Display(Name = "SqlServer_PasswordKey")]
        [StringLength(255)]
        public string PasswordKey { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description { get; set; }
    }

    public class SqlServer : SqlServerForSave
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
