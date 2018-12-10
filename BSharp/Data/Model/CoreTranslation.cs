using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BSharp.Services.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BSharp.Data.Model
{
    /// <summary>
    /// Represents a core translation, shared across all tenants
    /// </summary>
    public class CoreTranslation
    {
        [Required]
        [MaxLength(50)]
        public string Tier { get; set; } // Client, C#, SQL, Other

        [Required]
        [MaxLength(50)]
        public string Culture { get; set; } // ar-SA, en-GB, en, uz-Cyrl-UZ

        [Required]
        [MaxLength(450)]
        public string Name { get; set; } // The resource key

        [Required]
        [MaxLength(2048)]
        public string Value { get; set; } // The resource value

        internal static void OnModelCreating(ModelBuilder builder)
        {
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
        // Other languages can be added at runtime by localizing all the below codes
        internal static CoreTranslation[] TRANSLATIONS = {

            // Server Errors
            En(Constants.Server, "TheCode{0}IsDuplicated", "The code '{0}' is duplicated"),
            Ar(Constants.Server, "TheCode{0}IsDuplicated", "الكود ’{0}’ مكرر"),

            En(Constants.Server, "TheCode{0}IsUsed", "The code '{0}' is already used"),
            Ar(Constants.Server, "TheCode{0}IsUsed", "الكود ’{0}’ مستخدم حاليا"),

            En(Constants.Server, "Error.EmptyImportFile", "The imported file is empty"),
            Ar(Constants.Server, "Error.EmptyImportFile", "الملف المحمل ليس فيه بيانات"),

            En(Constants.Server, "Error.UnknownFileFormat", "Unknown file format"),
            Ar(Constants.Server, "Error.UnknownFileFormat", "صيغة الملف غير معروفة"),

            En(Constants.Server, "Error.ExcelContainsMultipleSheetsNameOne{0}", "The imported Excel file contains multiple sheets, please mark one of them with the name '{0}'"),
            Ar(Constants.Server, "Error.ExcelContainsMultipleSheetsNameOne{0}", "ملف الإكسل الذي رفعته يحتوي على أوراق متعدده، سم إحداهن بالاسم ’{0}’"),


            // Field Labels
            En(Constants.Shared, "MeasurementUnit_Code", "Code"),
            Ar(Constants.Shared, "MeasurementUnit_Code", "الكود"),

            En(Constants.Shared, "MeasurementUnit_UnitType", "Unit Type"),
            Ar(Constants.Shared, "MeasurementUnit_UnitType", "التصنيف"),

            En(Constants.Shared, "MeasurementUnit_UnitAmount", "Amount in this Unit"),
            Ar(Constants.Shared, "MeasurementUnit_UnitAmount", "الكمية بالوحدة الحالية"),

            En(Constants.Shared, "MeasurementUnit_BaseAmount", "Amount in base Unit"),
            Ar(Constants.Shared, "MeasurementUnit_BaseAmount", "الكمية بالوحدة الأساسية"),

            En(Constants.Shared, "MeasurementUnit_IsActive", "Is Active"),
            Ar(Constants.Shared, "MeasurementUnit_IsActive", "منشط"),

            En(Constants.Shared, "CreatedBy", "Created By"),
            Ar(Constants.Shared, "CreatedBy", "الإنشاء من قبل"),

            En(Constants.Shared, "CreatedAt", "Created At"),
            Ar(Constants.Shared, "CreatedAt", "زمن الإنشاء"),

            En(Constants.Shared, "ModifiedBy", "Modified By"),
            Ar(Constants.Shared, "ModifiedBy", "آخر تعديل من قبل"),

            En(Constants.Shared, "ModifiedAt", "Modified At"),
            Ar(Constants.Shared, "ModifiedAt", "زمن آخر تعديل"),

            En(Constants.Shared, "Data", "Data"),
            Ar(Constants.Shared, "Data", "البيانات"),


            // Choice lists
            En(Constants.Shared, "Yes", "Yes"),
            Ar(Constants.Shared, "Yes", "نعم"),

            En(Constants.Shared, "No", "No"),
            Ar(Constants.Shared, "No", "لا"),
        };

        private static CoreTranslation En(string tier, string name, string value)
        {
            return Lang(tier, "en", name, value);
        }

        private static CoreTranslation Ar(string tier, string name, string value)
        {
            return Lang(tier, "ar", name, value);
        }

        private static CoreTranslation Lang(string tier, string culture, string name, string value)
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
