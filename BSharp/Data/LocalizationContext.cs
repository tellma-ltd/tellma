using BSharp.Data.Model.Localization;
using BSharp.Services.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BSharp.Data
{
    public class LocalizationContext : DbContext
    {
        public LocalizationContext(DbContextOptions<LocalizationContext> opt) : base(opt) { }

        public DbSet<CoreTranslation> CoreTranslations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CoreTranslation>()
                .HasKey(e => new { e.Tier, e.Culture, e.Name });

            // TODO replace with real stuff
            builder.Entity<CoreTranslation>()
                .HasData(new CoreTranslation
                {
                    Tier = Constants.CSharp,
                    Culture = "en",
                    Name = "CouldNotRetrieveTheRecordWithId{0}",
                    Value = "Sorry, could not retrieve the record with Id {0}"
                },
                new CoreTranslation
                {
                    Tier = Constants.CSharp,
                    Culture = "ar",
                    Name = "CouldNotRetrieveTheRecordWithId{0}",
                    Value = "المعذرة لم يتم العثور على البيان ذي المفتاح {0}"
                });
        }
    }
}
