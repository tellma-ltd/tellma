using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Admin
{
    [Display(Name = "SqlDatabase", GroupName = "SqlDatabases")]
    public class SqlDatabaseForSave : EntityWithKey<int>
    {
        [Display(Name = "SqlDatabase_DatabaseName")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string DatabaseName { get; set; }

        [Display(Name = "SqlDatabase_Server")]
        [Required]
        public int? ServerId { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description { get; set; }
    }

    public class SqlDatabase : SqlDatabaseForSave
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

        [Display(Name = "SqlDatabase_Server")]
        [ForeignKey(nameof(ServerId))]
        public SqlServer Server { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public AdminUser CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public AdminUser ModifiedBy { get; set; }
    }
}
