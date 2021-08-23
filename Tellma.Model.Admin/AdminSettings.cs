using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Admin
{
    public class AdminSettingsForSave : Entity
    {
    }

    public class AdminSettings : AdminSettingsForSave
    {
        /// <summary>
        /// Changes whenever the admin settings change.
        /// </summary>
        public Guid SettingsVersion { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset ModifiedAt { get; set; }

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
