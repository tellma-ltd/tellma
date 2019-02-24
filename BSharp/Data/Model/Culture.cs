using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace BSharp.Data.Model
{
    public class Culture
    {
        [Required]
        [MaxLength(255)]
        public string Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string NeutralName { get; set; } // The name without the region

        public Guid TranslationsVersion { get; set; }

        public bool IsActive { get; set; } // specifies whether this culture is active or not

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Just a random GUID
            Guid defaultGuid = new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd");

            // The database comes pre-filled with all the neutral cultures
            var defaultCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
                .Select(e => new Culture
                {
                    Id = e.Name,
                    Name = e.NativeName,
                    NeutralName = e.IsNeutralCulture ? e.NativeName : e.Parent.NativeName,
                    TranslationsVersion = defaultGuid,
                    IsActive = e.Name == "en"
                });

            builder.Entity<Culture>()
                .HasData(defaultCultures);

            builder.Entity<Culture>()
                .Property(e => e.TranslationsVersion)
                .HasDefaultValue(defaultGuid);
        }
    }
}
