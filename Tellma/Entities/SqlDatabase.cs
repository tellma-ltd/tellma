using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "SqlDatabase", Plural = "SqlDatabases")]
    public class SqlDatabaseForSave : EntityWithKey<int>
    {
        [Display(Name = "SqlDatabase_DatabaseName")]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string DatabaseName { get; set; }

        [Display(Name = "SqlDatabase_Server")]
        [NotNull]
        [AlwaysAccessible]
        public int? ServerId { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description { get; set; }
    }

    public class SqlDatabase : SqlDatabaseForSave
    {

        [Display(Name = "CreatedAt")]
        [NotNull]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [NotNull]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
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
