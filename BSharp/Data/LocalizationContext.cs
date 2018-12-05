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
                .HasKey(e => new { e.Culture, e.Name });


            // Note: Should NEVER mix migrations seeding with startup seeding
            // The plan is to keep the seeding of localization in startup in the early days of development
            // since the localizations data will change and grow very frequently, once that data is stable
            // we switch to migration seeding

            //builder.Entity<CoreTranslation>()
            //    .HasData(_TRANSLATIONS);
        }

        // Note: English language comes built into the application, we also add Arabic for development
        // purposes to test localization where one language is RTL, so Arabic also ends up being built-in
        public static CoreTranslation[] _TRANSLATIONS = {
            En(Constants.Server, "TheCode{0}IsDuplicated", "The code '{0}' is duplicated"),
            Ar(Constants.Server, "TheCode{0}IsDuplicated", "الكود ’{0}’ مكرر"),

            En(Constants.Server, "TheCode{0}IsUsed", "The code '{0}' is already used"),
            Ar(Constants.Server, "TheCode{0}IsUsed", "الكود ’{0}’ مستخدم حاليا"),

        };

        public static CoreTranslation En(string tier, string name, string value)
        {
            return Lang(tier, "en", name, value);
        }

        public static CoreTranslation Ar(string tier, string name, string value)
        {
            return Lang(tier, "ar", name, value);
        }

        public static CoreTranslation Lang(string tier, string culture, string name, string value)
        {
            return new CoreTranslation
            {
                Tier = tier,
                Culture = culture,
                Name = name,
                Value = value
            };
        }
    }
}
