using BSharp.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public static CoreTranslation[] TRANSLATIONS = {

            // Built-in from Microsoft Libraries

            En(Constants.Server, nameof(RequiredAttribute), "The {0} field is required."),
            Ar(Constants.Server, nameof(RequiredAttribute), "حقل {0} مطلوب"),

            En(Constants.Server, nameof(StringLengthAttribute), "The field {0} must be a string with a maximum length of {1}."),
            Ar(Constants.Server, nameof(StringLengthAttribute), "حقل {0} ينبغي ألا يتعدى طول محنواه {1} حرفا"),


            // Server Errors

            En(Constants.Server, "Error_TheCode0IsDuplicated", "The code '{0}' is duplicated"),
            Ar(Constants.Server, "Error_TheCode0IsDuplicated", "الكود ’{0}’ مكرر"),

            En(Constants.Server, "Error_TheCode0IsUsed", "The code '{0}' is already used"),
            Ar(Constants.Server, "Error_TheCode0IsUsed", "الكود ({0}) مستخدم حاليا"),

            En(Constants.Server, "Error_TheName0IsDuplicated", "The name '{0}' is duplicated"),
            Ar(Constants.Server, "Error_TheName0IsDuplicated", "الاسم ({0}) مكرر"),

            En(Constants.Server, "Error_TheName0IsUsed", "The name '{0}' is already used"),
            Ar(Constants.Server, "Error_TheName0IsUsed", "الاسم ({0}) مستخدم حاليا"),


            En(Constants.Server, "Error_TheName20IsDuplicated", "The name '{0}' is duplicated"),
            Ar(Constants.Server, "Error_TheName20IsDuplicated", "الاسم ({0}) مكرر"),

            En(Constants.Server, "Error_TheName20IsUsed", "The second name '{0}' is already used"),
            Ar(Constants.Server, "Error_TheName20IsUsed", "الاسم الثاني ({0}) مستخدم حاليا"),


            En(Constants.Server, "Error_TheEntityWithId0IsSpecifiedMoreThanOnce", "The entity with Id '{0}' is specified more than once"),
            Ar(Constants.Server, "Error_TheEntityWithId0IsSpecifiedMoreThanOnce", "البيان ذو المفتاح ({0}) مذكور أكثر من مرة"),

            En(Constants.Server, "Error_Deleting0IsNotSupportedFromThisAPI", "Deleting {0} is not supported from this API"),
            Ar(Constants.Server, "Error_Deleting0IsNotSupportedFromThisAPI", "حذف {0} ليس مدعوما من هذه الواجهة"),

            En(Constants.Server, "Error_CannotInsert0WhileSpecifyId", "Cannot insert a {0} while specifying its Id"),
            Ar(Constants.Server, "Error_CannotInsert0WhileSpecifyId", "لا يمكن إنشاء {0} مع تحديد المفتاح"),

            En(Constants.Server, "Error_CannotUpdate0WithoutId", "Cannot update a {0} without specifying its Id"),
            Ar(Constants.Server, "Error_CannotUpdate0WithoutId", "لا يمكن نعديل {0} بدون تحديد المفتاح"),

            En(Constants.Server, "Error_CodeIsRequiredForImportModeUpdateAndDelete", "The code is required for import modes Update and Delete"),
            Ar(Constants.Server, "Error_CodeIsRequiredForImportModeUpdateAndDelete", "الكود مطلوب لوضعي الاستيراد التعديل والحذف"),

            En(Constants.Server, "Error_TheUnitCode0DoesNotExist", "The unit code '{0}' does not exist"),
            Ar(Constants.Server, "Error_TheUnitCode0DoesNotExist", "الكود ({0}) غير موجود بين أكواد الوحدات"),

            En(Constants.Server, "Error_EmptyImportFile", "The imported file is empty"),
            Ar(Constants.Server, "Error_EmptyImportFile", "الملف المحمل ليس فيه بيانات"),

            En(Constants.Server, "Error_UnknownFileFormat", "Unknown file format"),
            Ar(Constants.Server, "Error_UnknownFileFormat", "صيغة الملف غير معروفة"),

            En(Constants.Server, "Error_ExcelContainsMultipleSheetsNameOne0", "The imported Excel file contains multiple sheets, please mark one of them with the name '{0}'"),
            Ar(Constants.Server, "Error_ExcelContainsMultipleSheetsNameOne0", "ملف الإكسل الذي رفعته يحتوي على أوراق متعدده، سم إحداهن بالاسم ({0})"),

            En(Constants.Server, "Error_Column0NotRecognizable", "The column '{0}' is not recognizable"),
            Ar(Constants.Server, "Error_Column0NotRecognizable", "عنوان العمود ({0}) غير معروف"),

            En(Constants.Server, "Error_Value0IsNotValidFor1AcceptableValuesAre2", "The value '{0}' is not valid for the {1} field, acceptable values are: {2}"),
            Ar(Constants.Server, "Error_Value0IsNotValidFor1AcceptableValuesAre2", "القيمة ({0}) ليست صالحة لحقل {1}، القيم الصالحة هي: {2}"),

            En(Constants.Server, "Error_CannotDelete0AlreadyInUse", "Cannot delete a {0} record because it is already in use"),
            Ar(Constants.Server, "Error_CannotDelete0AlreadyInUse", "تعذر حذف بيان من نوع {0} لأنه سبق استخدامه"),

            
            // Field Labels

            En(Constants.Shared, "CreatedBy", "Created By"),
            Ar(Constants.Shared, "CreatedBy", "الإنشاء من قبل"),

            En(Constants.Shared, "CreatedAt", "Created At"),
            Ar(Constants.Shared, "CreatedAt", "زمن الإنشاء"),

            En(Constants.Shared, "ModifiedBy", "Modified By"),
            Ar(Constants.Shared, "ModifiedBy", "آخر تعديل من قبل"),

            En(Constants.Shared, "ModifiedAt", "Modified At"),
            Ar(Constants.Shared, "ModifiedAt", "زمن آخر تعديل"),

            En(Constants.Shared, "MeasurementUnit", "Measurement Unit"),
            Ar(Constants.Shared, "MeasurementUnit", "وحدة قياس"),

            En(Constants.Shared, "MeasurementUnits", "Measurement Units"),
            Ar(Constants.Shared, "MeasurementUnits", "وحدات قياس"),

            En(Constants.Shared, "MU_Name", "Name"),
            Ar(Constants.Shared, "MU_Name", "الاسم"),

            En(Constants.Shared, "MU_Name2", "Second Name"),
            Ar(Constants.Shared, "MU_Name2", "الاسم الثاني"),

            En(Constants.Shared, "MU_Code", "Code"),
            Ar(Constants.Shared, "MU_Code", "الكود"),

            En(Constants.Shared, "MU_UnitType", "Unit Type"),
            Ar(Constants.Shared, "MU_UnitType", "التصنيف"),

            En(Constants.Shared, "MU_UnitAmount", "Amount in this Unit"),
            Ar(Constants.Shared, "MU_UnitAmount", "الكمية بالوحدة الحالية"),

            En(Constants.Shared, "MU_BaseAmount", "Amount in base Unit"),
            Ar(Constants.Shared, "MU_BaseAmount", "الكمية بالوحدة الأساسية"),

            En(Constants.Shared, "MU_IsActive", "Is Active"),
            Ar(Constants.Shared, "MU_IsActive", "منشط"),

            En(Constants.Shared, "Data", "Data"),
            Ar(Constants.Shared, "Data", "البيانات"),

            En(Constants.Shared, "Row{0}", "Row {0}"),
            Ar(Constants.Shared, "Row{0}", "السطر {0}"),


            // Choice lists

            En(Constants.Shared, "MU_Pure", "Pure"),
            Ar(Constants.Shared, "MU_Pure", "محض"),

            En(Constants.Shared, "MU_Time", "Time"),
            Ar(Constants.Shared, "MU_Time", "زمن"),

            En(Constants.Shared, "MU_Distance", "Distance"),
            Ar(Constants.Shared, "MU_Distance", "مسافة"),

            En(Constants.Shared, "MU_Count", "Count"),
            Ar(Constants.Shared, "MU_Count", "عدد"),

            En(Constants.Shared, "MU_Mass", "Mass"),
            Ar(Constants.Shared, "MU_Mass", "كتلة"),

            En(Constants.Shared, "MU_Volume", "Volume"),
            Ar(Constants.Shared, "MU_Volume", "حجم"),

            En(Constants.Shared, "MU_Money", "Money"),
            Ar(Constants.Shared, "MU_Money", "نقد"),


            En(Constants.Shared, ",", ","),
            Ar(Constants.Shared, ",", "،"),

            //En(Constants.Shared, "Mode_Insert", "Insert"),
            //Ar(Constants.Shared, "Mode_Insert", "إضافة"),

            //En(Constants.Shared, "Mode_Update", "Update"),
            //Ar(Constants.Shared, "Mode_Update", "تعديل"),

            //En(Constants.Shared, "Mode_Merge", "Merge"),
            //Ar(Constants.Shared, "Mode_Merge", "دمج"),

            //En(Constants.Shared, "Mode_Delete", "Delete"),
            //Ar(Constants.Shared, "Mode_Delete", "حذف"),
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
