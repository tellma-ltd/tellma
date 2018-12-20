using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public string Symbol { get; set; } // One or Two letter symbol to represent the language e.g. En and ع

        public bool IsActive { get; set; } // specifies whether this culture is active or not

        internal static void OnModelCreating(ModelBuilder builder)
        {
            var defaultCultures = new Culture[]
            {
                new Culture {
                    Id = "en",
                    Name = "English",
                    Symbol = "En",
                    IsActive = true
                },
                new Culture {
                    Id = "ar",
                    Name = "العربية",
                    Symbol = "ع" ,
                    IsActive = true
                },
            };

            builder.Entity<Culture>()
                .HasData(defaultCultures);
        }
    }
}
