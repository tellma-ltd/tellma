using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "SqlServer", Plural = "SqlServers")]
    public class SqlServerForSave : EntityWithKey<int>
    {
        [Display(Name = "SqlServer_ServerName")]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string ServerName { get; set; }

        [Display(Name = "SqlServer_UserName")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string UserName { get; set; }

        [Display(Name = "SqlServer_PasswordKey")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string PasswordKey { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description { get; set; }
    }

    public class SqlServer : SqlServerForSave
    {
        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
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
