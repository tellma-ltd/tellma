using BSharp.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Data.Model
{
    /// <summary>
    /// Represents a core translation, shared across all tenants
    /// </summary>
    public class Translation : ModelBase
    {
        [Required]
        [MaxLength(255)]
        public string CultureId { get; set; } // ar-SA, en-GB, en, uz-Cyrl-UZ
        public Culture Culture { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } // The resource key

        [Required]
        [MaxLength(2048)]
        public string Value { get; set; } // The resource value

        [Required]
        [MaxLength(255)]
        public string Tier { get; set; } // Client, Server, Shared

        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Translation>()
                .HasKey(e => new { e.CultureId, e.Name });

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
        public static Translation[] TRANSLATIONS = {

            // Built-in from Microsoft Libraries

            En(Constants.Server, nameof(RequiredAttribute), "The {0} field is required."),
            Ar(Constants.Server, nameof(RequiredAttribute), "حقل {0} مطلوب"),

            En(Constants.Server, nameof(StringLengthAttribute), "The field {0} must be a string with a maximum length of {1}"),
            Ar(Constants.Server, nameof(StringLengthAttribute), "حقل {0} ينبغي ألا يتعدى طول محنواه {1} حرفا"),

            En(Constants.Server, nameof(EmailAddressAttribute), "The {0} field is not a valid e-mail address"),
            Ar(Constants.Server, nameof(EmailAddressAttribute), "حقل {0} لا يحتوي على عنوان بريد سليم"),            

            // Server Errors
            En(Constants.Server, "Error_TheId0WasNotFound", "The record with Id '{0}' was not found. Perhaps it was already deleted, please try refreshing"),
            Ar(Constants.Server, "Error_TheId0WasNotFound", "البيان ذو المفتاح ({0}) غير موجود، لعل بعضهم حذفه، يرجى محاولة التحديث"),

            En(Constants.Server, "Error_TheCode0IsDuplicated", "The code '{0}' is duplicated"),
            Ar(Constants.Server, "Error_TheCode0IsDuplicated", "الكود ({0}) مكرر"),

            En(Constants.Server, "Error_TheCode0IsUsed", "The code '{0}' is already used"),
            Ar(Constants.Server, "Error_TheCode0IsUsed", "الكود ({0}) مستخدم حاليا"),

            En(Constants.Server, "Error_TheEmail0IsUsed", "The email '{0}' is already used"),
            Ar(Constants.Server, "Error_TheEmail0IsUsed", "عنوان البريد ({0}) مستخدم حاليا"),

            En(Constants.Server, "Error_CannotModifyInactiveItem", "Cannot modify an inactive item"),
            Ar(Constants.Server, "Error_CannotModifyInactiveItem", "لا يمكن تعديل بيان غير منشط"),

            En(Constants.Server, "Error_TheName0IsDuplicated", "The name '{0}' is duplicated"),
            Ar(Constants.Server, "Error_TheName0IsDuplicated", "الاسم ({0}) مكرر"),

            En(Constants.Server, "Error_TheName0IsUsed", "The name '{0}' is already used"),
            Ar(Constants.Server, "Error_TheName0IsUsed", "الاسم ({0}) مستخدم حاليا"),

            En(Constants.Server, "Error_TheEntityWithId0IsSpecifiedMoreThanOnce", "The entity with Id '{0}' is specified more than once"),
            Ar(Constants.Server, "Error_TheEntityWithId0IsSpecifiedMoreThanOnce", "البيان ذو المفتاح ({0}) مذكور أكثر من مرة"),

            En(Constants.Server, "Error_Deleting0IsNotSupportedFromThisAPI", "Deleting {0} is not supported from this API"),
            Ar(Constants.Server, "Error_Deleting0IsNotSupportedFromThisAPI", "حذف {0} ليس مدعوما من هذه الواجهة"),

            En(Constants.Server, "Error_CannotInsertWhileSpecifyId", "Cannot insert an item while specifying its Id"),
            Ar(Constants.Server, "Error_CannotInsertWhileSpecifyId", "لا يمكن إنشاء بيان مع تحديد المفتاح"),

            En(Constants.Server, "Error_CannotUpdateWithoutId", "Cannot update an item without specifying its Id"),
            Ar(Constants.Server, "Error_CannotUpdateWithoutId", "لا يمكن نعديل بيان بدون تحديد المفتاح"),

            En(Constants.Server, "Error_CannotDeleteWithoutId", "Cannot delete an item without specifying its Id"),
            Ar(Constants.Server, "Error_CannotDeleteWithoutId", "لا يمكن حذف بيان بدون تحديد المفتاح"),

            En(Constants.Server, "Error_CodeIsRequiredForImportModeUpdate", "The code is required for the update import mode"),
            Ar(Constants.Server, "Error_CodeIsRequiredForImportModeUpdate", "الكود مطلوب لوضع التعديل"),

            En(Constants.Server, "Error_TheCode0DoesNotExist", "The code '{0}' does not exist"),
            Ar(Constants.Server, "Error_TheCode0DoesNotExist", "الكود ({0}) غير موجود"),

            En(Constants.Server, "Error_TheView0IsInactive", "The view with code '{0}' is not activated"),
            Ar(Constants.Server, "Error_TheView0IsInactive", "الواجهة ذات الكود ({0}) غير منشطة"),
            
            En(Constants.Server, "Error_NoFileWasUploaded", "No file was uploaded"),
            Ar(Constants.Server, "Error_NoFileWasUploaded", "لم يتم رفع أي ملف"),

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

            En(Constants.Server, "Error_ParsingForDeleteIsNotSupported", "Delete mode is not supported in the parsing API"),
            Ar(Constants.Server, "Error_ParsingForDeleteIsNotSupported", "وضع الحذف ليس مدعوما من هذه الواجهة"),

            En(Constants.Server, "Error_TheValue0IsNotValidFor1Field", "The value '{0}' is not valid for the {1} field"),
            Ar(Constants.Server, "Error_TheValue0IsNotValidFor1Field", "القيمة ({0}) ليست صالحة لحقل {1}"),


            // Client Errors
            En(Constants.Client, "Error_UnableToReachServer", "Unable to reach the server, please check the connection of your device"),
            Ar(Constants.Client, "Error_UnableToReachServer", "تعذر الوصول إلى الخادم، يرجى التأكد من اتصال جهازك بالشبكة"),

            En(Constants.Client, "Error_LoginSessionExpired", "Your login session has expired, please login again"),
            Ar(Constants.Client, "Error_LoginSessionExpired", "إنتهت صلاحية تسجيل دخولك، يرجى تسحيل الدخول من جديد"),

            En(Constants.Client, "Error_AccountDoesNotHaveSufficientPermissions", "Your account does not have sufficient permissions"),
            Ar(Constants.Client, "Error_AccountDoesNotHaveSufficientPermissions", "حسابك على النظام لا يتمتع بالأذونات الكافية"),

            En(Constants.Client, "Error_RecordNotFound", "The specified record was not found"),
            Ar(Constants.Client, "Error_RecordNotFound", "لم يتم العثور على البيان المطلوب"),

            En(Constants.Client, "Error_UnhandledServerError", "An unhandled error occurred on the server, please contact your IT department"),
            Ar(Constants.Client, "Error_UnhandledServerError", "حدث خطأ غير معالج على على الخادم، يرجى مراجعة إدارة المعلومات"),

            En(Constants.Client, "Error_UnkownServerError", "An unknown error occurred on the server, please contact your IT department"),
            Ar(Constants.Client, "Error_UnkownServerError", "حدث خطأ غير معروف على على الخادم، يرجى مراجعة إدارة المعلومات"),

            En(Constants.Client, "Error_UnkownClientError", "An unknown error occurred on the client, please contact your IT department"),
            Ar(Constants.Client, "Error_UnkownClientError", "حدث خطأ غير معروف على على النظام العميل، يرجى مراجعة إدارة المعلومات"),
            
            
            // Labels
            En(Constants.Shared, "AppName", "BSharp"),
            Ar(Constants.Shared, "AppName", "إياس"),

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

            En(Constants.Shared, "MU_UnitType", "Unit Type"),
            Ar(Constants.Shared, "MU_UnitType", "التصنيف"),

            En(Constants.Shared, "MU_UnitAmount", "Amount in this Unit"),
            Ar(Constants.Shared, "MU_UnitAmount", "الكمية بالوحدة الحالية"),

            En(Constants.Shared, "MU_BaseAmount", "Amount in base Unit"),
            Ar(Constants.Shared, "MU_BaseAmount", "الكمية بالوحدة الأساسية"),

            En(Constants.Shared, "Custody", "Custody"),
            Ar(Constants.Shared, "Custody", "عهدة"),

            En(Constants.Shared, "Custodies", "Custodies"),
            Ar(Constants.Shared, "Custodies", "عُهد"),

            En(Constants.Shared, "Custody_Address", "Address"),
            Ar(Constants.Shared, "Custody_Address", "العنوان"),

            En(Constants.Shared, "Custody_CustodyType", "Custody Type"),
            Ar(Constants.Shared, "Custody_CustodyType", "نوع العهدة"),

            En(Constants.Shared, "Individual", "Individual"),
            Ar(Constants.Shared, "Individual", "فرد"),

            En(Constants.Shared, "Organization", "Organization"),
            Ar(Constants.Shared, "Organization", "مؤسسة"),

            En(Constants.Shared, "Individuals", "Individuals"),
            Ar(Constants.Shared, "Individuals", "أفراد"),

            En(Constants.Shared, "Organizations", "Organizations"),
            Ar(Constants.Shared, "Organizations", "مؤسسات"),

            En(Constants.Shared, "Agent_AgentType", "Agent Type"),
            Ar(Constants.Shared, "Agent_AgentType", "نوع الذمة"),

            En(Constants.Shared, "Agent_IsRelated", "Is Related"),
            Ar(Constants.Shared, "Agent_IsRelated", "ذو علاقة"),

            En(Constants.Shared, "Agent_UserId", "User"),
            Ar(Constants.Shared, "Agent_UserId", "المستخدم"),

            En(Constants.Shared, "Agent_TaxIdentificationNumber", "Tax ID Number"),
            Ar(Constants.Shared, "Agent_TaxIdentificationNumber", "رقم السجل الضريبي"),

            En(Constants.Shared, "Agent_Title", "Title"),
            Ar(Constants.Shared, "Agent_Title", "اللقب"),

            En(Constants.Shared, "Agent_Title2", "Second Title"), // TODO
            Ar(Constants.Shared, "Agent_Title2", "اللقب الثاني"),

            En(Constants.Shared, "Agent_individuals_BirthDateTime", "Date of Birth"),
            Ar(Constants.Shared, "Agent_individuals_BirthDateTime", "تاريخ الميلاد"),

            En(Constants.Shared, "Agent_organizations_BirthDateTime", "Date of Establishment"),
            Ar(Constants.Shared, "Agent_organizations_BirthDateTime", "تاريخ التأسيس"),

            En(Constants.Shared, "Agent_Gender", "Gender"),
            Ar(Constants.Shared, "Agent_Gender", "الجنس"),

            En(Constants.Shared, "View", "View"),
            Ar(Constants.Shared, "View", "واجهة"),

            En(Constants.Shared, "Views", "Views"),
            Ar(Constants.Shared, "Views", "واجهات"),
            
            En(Constants.Shared, "Role", "Role"),
            Ar(Constants.Shared, "Role", "دور"),

            En(Constants.Shared, "Roles", "Roles"),
            Ar(Constants.Shared, "Roles", "أدوار"),

            En(Constants.Shared, "Permission", "Permission"),
            Ar(Constants.Shared, "Permission", "إذن"),

            En(Constants.Shared, "Permissions", "Permissions"),
            Ar(Constants.Shared, "Permissions", "أذونات"),

            En(Constants.Shared, "Role_IsPublic", "Is Public"),
            Ar(Constants.Shared, "Role_IsPublic", "عام للجميع"),

            En(Constants.Shared, "Permission_View", "View"),
            Ar(Constants.Shared, "Permission_View", "الواجهة"),

            En(Constants.Shared, "Permission_Role", "Role"),
            Ar(Constants.Shared, "Permission_Role", "الدور"),

            En(Constants.Shared, "Permission_Level", "Level"),
            Ar(Constants.Shared, "Permission_Level", "الدرجة"),

            En(Constants.Shared, "Permission_Read", "Read"),
            Ar(Constants.Shared, "Permission_Read", "اطلاع"),

            En(Constants.Shared, "Permission_Update", "Update"),
            Ar(Constants.Shared, "Permission_Update", "تعديل"),

            En(Constants.Shared, "Permission_Create", "Create"),
            Ar(Constants.Shared, "Permission_Create", "إنشاء"),

            En(Constants.Shared, "Permission_ReadAndCreate", "Read and Create"),
            Ar(Constants.Shared, "Permission_ReadAndCreate", "اطلاع وإنشاء"),

            En(Constants.Shared, "Permission_Sign", "Sign"),
            Ar(Constants.Shared, "Permission_Sign", "توقيع"),

            En(Constants.Shared, "Permission_Criteria", "Criteria"),
            Ar(Constants.Shared, "Permission_Criteria", "تقييد"),

            En(Constants.Shared, "User", "User"),
            Ar(Constants.Shared, "User", "مستخدم"),

            En(Constants.Shared, "Users", "Users"),
            Ar(Constants.Shared, "Users", "مستخدمون"),

            En(Constants.Shared, "User_Email", "Email"),
            Ar(Constants.Shared, "User_Email", "البريد الإلكتروني"),

            En(Constants.Shared, "User_Agent", "Agent"),
            Ar(Constants.Shared, "User_Agent", "الذمّة"),

            En(Constants.Shared, "User_Roles", "Roles"),
            Ar(Constants.Shared, "User_Roles", "الأدوار"),

            En(Constants.Shared, "User_Companies", "Companies"),
            Ar(Constants.Shared, "User_Companies", "الشركات"),

            En(Constants.Shared, "RoleMembership_User", "User"),
            Ar(Constants.Shared, "RoleMembership_User", "المستخدم"),

            En(Constants.Shared, "RoleMembership_Role", "Role"),
            Ar(Constants.Shared, "RoleMembership_Role", "الدور"),

            En(Constants.Shared, "Memo", "Memo"),
            Ar(Constants.Shared, "Memo", "ملاحظات"),

            En(Constants.Shared, "Name", "Name"),
            Ar(Constants.Shared, "Name", "الاسم"),

            En(Constants.Shared, "Name2", "Second Name"), // TODO
            Ar(Constants.Shared, "Name2", "الاسم الثاني"),

            En(Constants.Shared, "Code", "Code"),
            Ar(Constants.Shared, "Code", "الكود"),

            En(Constants.Shared, "IsActive", "Is Active"),
            Ar(Constants.Shared, "IsActive", "منشط"),

            En(Constants.Shared, "Data", "Data"),
            Ar(Constants.Shared, "Data", "البيانات"),

            En(Constants.Shared, "Row{0}", "Row {0}"),
            Ar(Constants.Shared, "Row{0}", "السطر {0}"),

            En(Constants.Shared, "T_Value", "Value"),
            Ar(Constants.Shared, "T_Value", "القيمة"),

            En(Constants.Shared, "T_Name", "Name"),
            Ar(Constants.Shared, "T_Name", "الاسم"),

            En(Constants.Shared, "T_CultureId", "Culture"),
            Ar(Constants.Shared, "T_CultureId", "اللغة"),

            En(Constants.Shared, "T_Tier", "Tier"),
            Ar(Constants.Shared, "T_Tier", "الطبقة"),

            En(Constants.Shared, "Signatures", "Signatures"),
            Ar(Constants.Shared, "Signatures", "توقيعات"),

            En(Constants.Shared, "Members", "Members"),
            Ar(Constants.Shared, "Members", "أعضاء"),

            En(Constants.Shared, "View_All", "All"),
            Ar(Constants.Shared, "View_All", "الجميع"),

            En(Constants.Client, "Search", "Search"),
            Ar(Constants.Client, "Search", "بحث"),

            En(Constants.Client, "Create", "Create"),
            Ar(Constants.Client, "Create", "إنشاء"),

            En(Constants.Client, "Edit", "Edit"),
            Ar(Constants.Client, "Edit", "تعديل"),

            En(Constants.Client, "Clone", "Clone"),
            Ar(Constants.Client, "Clone", "استنساخ"),

            En(Constants.Client, "Delete", "Delete"),
            Ar(Constants.Client, "Delete", "حذف"),

            En(Constants.Client, "Import", "Import"),
            Ar(Constants.Client, "Import", "استيراد"),

            En(Constants.Client, "Export", "Export"),
            Ar(Constants.Client, "Export", "تصدير"),

            En(Constants.Client, "Filter", "Filter"),
            Ar(Constants.Client, "Filter", "تصفية"),

            En(Constants.Client, "IncludeInactive", "Include Inactive"),
            Ar(Constants.Client, "IncludeInactive", "إشمل غير المنشط"),

            En(Constants.Client, "Public", "Public"),
            Ar(Constants.Client, "Public", "عام للجميع"),

            En(Constants.Client, "NotPublic", "Not Public"),
            Ar(Constants.Client, "NotPublic", "غير عام للجميع"),

            En(Constants.Client, "WithAgent", "With Agent"),
            Ar(Constants.Client, "WithAgent", "ذمة معرفة"),

            En(Constants.Client, "WithoutAgent", "Without Agent"),
            Ar(Constants.Client, "WithoutAgent", "ذمة غير معرفة"),

            En(Constants.Client, "ExportRange", "Exporting Range"),
            Ar(Constants.Client, "ExportRange", "نطاق التصدير"),

            En(Constants.Client, "DownloadTemplate", "Download Template"),
            Ar(Constants.Client, "DownloadTemplate", "تحميل القالب"),

            En(Constants.Client, "Template", "Template"),
            Ar(Constants.Client, "Template", "قالب"),

            En(Constants.Client, "Refresh", "Refresh"),
            Ar(Constants.Client, "Refresh", "تحديث"),

            En(Constants.Client, "Loading", "Loading"),
            Ar(Constants.Client, "Loading", "جار التحميل"),

            En(Constants.Client, "New", "New"),
            Ar(Constants.Client, "New", "جديد"),

            En(Constants.Client, "RequiredField", "Required Field"),
            Ar(Constants.Client, "RequiredField", "حقل مطلوب"),

            En(Constants.Client, "InvalidDate", "Invalid Date"),
            Ar(Constants.Client, "InvalidDate", "تاريخ غير سليم"),

            En(Constants.Client, "Tiles", "Tiles"),
            Ar(Constants.Client, "Tiles", "مستطيلات"),

            En(Constants.Client, "Table", "Table"),
            Ar(Constants.Client, "Table", "جدول"),

            En(Constants.Client, "First", "First"),
            Ar(Constants.Client, "First", "الأول"),

            En(Constants.Client, "Previous", "Previous"),
            Ar(Constants.Client, "Previous", "السابق"),

            En(Constants.Client, "Next", "Next"),
            Ar(Constants.Client, "Next", "التالي"),

            En(Constants.Client, "NoItemsFound", "No Items Found."),
            Ar(Constants.Client, "NoItemsFound", "لا يوجد بيانات"),

            En(Constants.Client, "CreatedByMe", "Created By Me"),
            Ar(Constants.Client, "CreatedByMe", "منشأ من قبلي"),

            En(Constants.Client, "ImportAFile", "Import a File"),
            Ar(Constants.Client, "ImportAFile", "استيراد ملف"),

            En(Constants.Client, "Cancel", "Cancel"),
            Ar(Constants.Client, "Cancel", "إلغاء"),

            En(Constants.Client, "Save", "Save"),
            Ar(Constants.Client, "Save", "حفظ"),

            En(Constants.Client, "Proceed", "Proceed"),
            Ar(Constants.Client, "Proceed", "متأكد"),

            En(Constants.Client, "Mode", "Mode"),
            Ar(Constants.Client, "Mode", "الوضع"),

            En(Constants.Client, "Format", "Format"),
            Ar(Constants.Client, "Format", "الصيغة"),

            En(Constants.Client, "Excel", "Excel"),
            Ar(Constants.Client, "Excel", "إكسل"),

            En(Constants.Client, "CSV", "CSV"),
            Ar(Constants.Client, "CSV", "قيم مفرقة بفواصل (CSV)"),

            En(Constants.Client, "Step1", "Step 1"),
            Ar(Constants.Client, "Step1", "الخطوة الأولى"),

            En(Constants.Client, "Step2", "Step 2"),
            Ar(Constants.Client, "Step2", "الخطوة الثانية"),

            En(Constants.Client, "Deleted", "Deleted"),
            Ar(Constants.Client, "Deleted", "محذوف"),

            En(Constants.Client, "UndoDelete", "Undo Delete"),
            Ar(Constants.Client, "UndoDelete", "تراجع عن الحذف"),

            En(Constants.Client, "ShortMonth1", "Jan"),
            Ar(Constants.Client, "ShortMonth1", "يناير"),

            En(Constants.Client, "ShortMonth2", "Feb"),
            Ar(Constants.Client, "ShortMonth2", "فبراير"),

            En(Constants.Client, "ShortMonth3", "Mar"),
            Ar(Constants.Client, "ShortMonth3", "مارس"),

            En(Constants.Client, "ShortMonth4", "Apr"),
            Ar(Constants.Client, "ShortMonth4", "أبريل"),

            En(Constants.Client, "ShortMonth5", "May"),
            Ar(Constants.Client, "ShortMonth5", "مايو"),

            En(Constants.Client, "ShortMonth6", "Jun"),
            Ar(Constants.Client, "ShortMonth6", "يونيو"),

            En(Constants.Client, "ShortMonth7", "Jul"),
            Ar(Constants.Client, "ShortMonth7", "يوليو"),

            En(Constants.Client, "ShortMonth8", "Aug"),
            Ar(Constants.Client, "ShortMonth8", "أغسطس"),

            En(Constants.Client, "ShortMonth9", "Sep"),
            Ar(Constants.Client, "ShortMonth9", "سبتمبر"),

            En(Constants.Client, "ShortMonth10", "Oct"),
            Ar(Constants.Client, "ShortMonth10", "أكتوبر"),

            En(Constants.Client, "ShortMonth11", "Nov"),
            Ar(Constants.Client, "ShortMonth11", "نوفمبر"),

            En(Constants.Client, "ShortMonth12", "Dec"),
            Ar(Constants.Client, "ShortMonth12", "ديسمبر"),

            En(Constants.Client, "ShortDay1", "Mo"),
            Ar(Constants.Client, "ShortDay1", "إث"),

            En(Constants.Client, "ShortDay2", "Tu"),
            Ar(Constants.Client, "ShortDay2", "ثلا"),

            En(Constants.Client, "ShortDay3", "We"),
            Ar(Constants.Client, "ShortDay3", "أر"),

            En(Constants.Client, "ShortDay4", "Th"),
            Ar(Constants.Client, "ShortDay4", "خم"),

            En(Constants.Client, "ShortDay5", "Fr"),
            Ar(Constants.Client, "ShortDay5", "جم"),

            En(Constants.Client, "ShortDay6", "Sa"),
            Ar(Constants.Client, "ShortDay6", "ست"),

            En(Constants.Client, "ShortDay7", "Su"),
            Ar(Constants.Client, "ShortDay7", "أح"),

            En(Constants.Client, "ImportStep1Instructions", "Download the template file and populate it with your data:"),
            Ar(Constants.Client, "ImportStep1Instructions", "حمل ملف القالب على جهازك واملأه بالبيانات"),

            En(Constants.Client, "ImportStep2Instructions", "Specify the import mode and then upload the file back:"),
            Ar(Constants.Client, "ImportStep2Instructions", "حدد وضع الاستيراد المطلوب ومن ثم قم برفع الملف"),

            En(Constants.Client, "ImportedFileDidNotPassValidation", "The imported file did not pass validation, see below"),
            Ar(Constants.Client, "ImportedFileDidNotPassValidation", "الملف المستورد لم يستوف شروط الصحة، راجع التقرير أدناه"),

            En(Constants.Client, "ImportMergeSuccessMessage", "Successfully inserted {{Inserted}} record(s) and updated {{Updated}} record(s) in {{Seconds}} seconds"),
            Ar(Constants.Client, "ImportMergeSuccessMessage", "تمت أضافة عدد {{Inserted}} وتعديل عدد {{Updated}} من البيانات في {{Seconds}} من الثواني"),

            En(Constants.Client, "MaxExportSizeWarning", "Only {{max}} items can be exported at a time"),
            Ar(Constants.Client, "MaxExportSizeWarning", "التصدير محدود بعدد {{max}} من البيانات كحد أقصى"),

            En(Constants.Client, "NSelectedItems", "{{count}} selected Items"),
            Ar(Constants.Client, "NSelectedItems", "عدد البيانات المحددة {{count}}"),

            En(Constants.Client, "Actions", "Actions"),
            Ar(Constants.Client, "Actions", "أوامر"),

            En(Constants.Client, "Confirmation", "Confirmation"),
            Ar(Constants.Client, "Confirmation", "توكيد"),

            En(Constants.Client, "NewLine", "New Line"),
            Ar(Constants.Client, "NewLine", "سطر جديد"),

            En(Constants.Client, "DeleteConfirmationMessage", "Are you sure you want to delete {{count}} items? This action is irreversible."),
            Ar(Constants.Client, "DeleteConfirmationMessage", "هل تود حذف عدد {{count}} من البيانات؟ هذا الفعل لا يمكن التراجع عنه."),

            En(Constants.Client, "DetailsDeleteConfirmationMessage", "Are you sure you want to delete this item? This action is irreversible."),
            Ar(Constants.Client, "DetailsDeleteConfirmationMessage", "هل تود حذف هذا البيان؟ هذا الفعل لا يمكن التراجع عنه."),

            En(Constants.Client, "UnsavedChangesConfirmationMessage", "Your unsaved changes will be discarded, are you sure you would like to proceed?"),
            Ar(Constants.Client, "UnsavedChangesConfirmationMessage", "ستضيع التعديلات التي لم تُحفظ بعد، هل أنت متأكد؟"),

            En(Constants.Client, "PublicRoleWarning", "Warning! This role is public, therefore the permissions and signatures here are granted to ALL users"),
            Ar(Constants.Client, "PublicRoleWarning", "تنبيه! هذا الدور عام وبالتالي الأذونات والتوقيعات المعرفة هنا متاحة لجميع المستخدمين"),

            En(Constants.Client, "Activate", "Activate"),
            Ar(Constants.Client, "Activate", "تنشيط"),

            En(Constants.Client, "Deactivate", "Deactivate"),
            Ar(Constants.Client, "Deactivate", "إيقاف النشاط"),

            En(Constants.Client, "Error", "Error"),
            Ar(Constants.Client, "Error", "حدث خطأ"),

            En(Constants.Client, "Dismiss", "Dismiss"),
            Ar(Constants.Client, "Dismiss", "إزالة"),

            En(Constants.Client, "ClearFilter", "Clear Filter"),
            Ar(Constants.Client, "ClearFilter", "إزالة التصفية"),

            En(Constants.Client, "ActionDidNotPassValidation", "The action did not pass validation, see the highlighted rows for details"),
            Ar(Constants.Client, "ActionDidNotPassValidation", "الأمر المنفذ لم يستوف شروط الصحة، راجع الأسطر المؤشر عليها لمزيد من التفاصيل"),

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

            En(Constants.Shared, "Agent_Male", "Male"),
            Ar(Constants.Shared, "Agent_Male", "ذكر"),

            En(Constants.Shared, "Agent_Female", "Female"),
            Ar(Constants.Shared, "Agent_Female", "أنثى"),

            En(Constants.Shared, "Active", "Active"),
            Ar(Constants.Shared, "Active", "منشط"),

            En(Constants.Shared, "Inactive", "Inactive"),
            Ar(Constants.Shared, "Inactive", "غير منشط"),

            En(Constants.Shared, "Yes", "Yes"),
            Ar(Constants.Shared, "Yes", "نعم"),

            En(Constants.Shared, "No", "No"),
            Ar(Constants.Shared, "No", "لا"),

            En(Constants.Shared, ", ", ", "),
            Ar(Constants.Shared, ", ", "، "),

            En(Constants.Shared, "Mode_Insert", "Insert"),
            Ar(Constants.Shared, "Mode_Insert", "إضافة"),

            En(Constants.Shared, "Mode_Update", "Update"),
            Ar(Constants.Shared, "Mode_Update", "تعديل"),

            En(Constants.Shared, "Mode_Merge", "Merge"),
            Ar(Constants.Shared, "Mode_Merge", "دمج"),
        };

        private static Translation En(string tier, string name, string value)
        {
            return Lang(tier, "en", name, value);
        }

        private static Translation Ar(string tier, string name, string value)
        {
            return Lang(tier, "ar", name, value);
        }

        private static Translation Lang(string tier, string culture, string name, string value)
        {
            return new Translation
            {
                Tier = tier,
                CultureId = culture,
                Name = name,
                Value = value
            };
        }
    }
}
