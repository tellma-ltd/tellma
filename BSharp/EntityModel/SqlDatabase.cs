using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.EntityModel
{
    [StrongEntity]
    public class SqlDatabaseForSave : EntityWithKey<int>
    {
        [Display(Name = "SqlDatabase_DatabaseName")]
        [Required]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string DatabaseName { get; set; }

        [Display(Name = "SqlDatabase_Server")]
        [AlwaysAccessible]
        public int? ServerId { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        public string Description { get; set; }
    }

    public class SqlDatabase : SqlDatabaseForSave
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

        [Display(Name = "SqlDatabase_Server")]
        [ForeignKey(nameof(ServerId))]
        public SqlServer Server { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public GlobalUser CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public GlobalUser ModifiedBy { get; set; }
    }
}
