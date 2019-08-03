using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.Model
{
    public class Settings : ModelBase
    {
        // The tenant Id will be the ID

        // Company Name

        [Required]
        [MaxLength(255)]
        public string ShortCompanyName { get; set; }

        [MaxLength(255)]
        public string ShortCompanyName2 { get; set; }

        // Languages

        [Required]
        [MaxLength(255)]
        public string PrimaryLanguageId { get; set; }

        [MaxLength(255)]
        public string PrimaryLanguageSymbol { get; set; }

        [MaxLength(255)]
        public string SecondaryLanguageId { get; set; }

        [MaxLength(255)]
        public string SecondaryLanguageSymbol { get; set; }

        // Branding

        [MaxLength(255)]
        public string BrandColor { get; set; } // e.g. #0284AB        

        /// <summary>
        /// Changes whenever the client views and the specs change
        /// </summary>
        public Guid ViewsAndSpecsVersion { get; set; }

        /// <summary>
        /// Changes whenever the client settings change
        /// </summary>
        public Guid SettingsVersion { get; set; }

        /// <summary>
        /// When was this tenant provisioned
        /// </summary>
        public DateTimeOffset ProvisionedAt { get; set; }

        /// <summary>
        /// Audit Info
        /// </summary>
        public int ModifiedById { get; set; }

        /// <summary>
        /// Audit Info
        /// </summary>
        public DateTimeOffset ModifiedAt { get; set; }


        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Just a random GUID
            Guid defaultGuid = new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd");

            builder.Entity<Settings>().Property(e => e.SettingsVersion).HasDefaultValue(defaultGuid);
            builder.Entity<Settings>().Property(e => e.ViewsAndSpecsVersion).HasDefaultValue(defaultGuid);
        }
    }
}
