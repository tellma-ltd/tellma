using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Identity;
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

            En(Constants.Server, nameof(StringLengthAttribute) + "2", "The field {0} must be at least {2} and at max {1} characters long."),
            Ar(Constants.Server, nameof(StringLengthAttribute) + "2", "حقل {0} ينبغي ألا يقل طول محنواه عن عدد {2} من الحروف وألا يتجاوز عدد {1} من الحروف"),

            En(Constants.Server, nameof(EmailAddressAttribute), "The {0} field is not a valid e-mail address"),
            Ar(Constants.Server, nameof(EmailAddressAttribute), "حقل {0} لا يحتوي على عنوان بريد إلكتروني سليم"),            

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

            En(Constants.Server, "Error_TheUser0IsInactive", "The user '{0}' is not active"),
            Ar(Constants.Server, "Error_TheUser0IsInactive", "المستخدم ({0}) غير منشط"),

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

            En(Constants.Server, "Error_CannotDeactivateYourOwnUser", "You cannot deactivate your own user"),
            Ar(Constants.Server, "Error_CannotDeactivateYourOwnUser", "لا يمكنك إيقاف نشاط المستخدم خاصتك"),

            En(Constants.Server, "Error_CannotDeleteYourOwnUser", "You cannot delete your own user"),
            Ar(Constants.Server, "Error_CannotDeleteYourOwnUser", "لا يمكنك حذف المستخدم خاصتك"),

            En(Constants.Server, "Error_TheEmailCannotBeModified", "The user email cannot be modified from here"),
            Ar(Constants.Server, "Error_TheEmailCannotBeModified", "لا يمكن تعديل عنوان بريد المستخدم من هنا"),

            En(Constants.Server, "Error_TheField0MustBeAValidColorFormat", "The field {0} must be a valid HTML hexadecimal color, such as: #AB12E5"),
            Ar(Constants.Server, "Error_TheField0MustBeAValidColorFormat", "حقل {0} يجب أن يكون لون HTML سداسي عشري صالح، مثال: #AB12E5 "),

            En(Constants.Server, "Error_InvalidLanguageId0", "Invalid Language Id '{0}'"),
            Ar(Constants.Server, "Error_InvalidLanguageId0", "مفتاح اللغة ({0}) غير معروف"),

            En(Constants.Server, "Error_SecondaryLanguageCannotBeTheSameAsPrimaryLanguage", "Secondary language cannot be the same as the primary language"),
            Ar(Constants.Server, "Error_SecondaryLanguageCannotBeTheSameAsPrimaryLanguage", "اللغة الثانوية هي نفسها اللغة الرئيسية"),
            
            
            // Client Errors
            En(Constants.Client, "Error_UnableToReachServer", "Unable to reach the server, please check the connection of your device"),
            Ar(Constants.Client, "Error_UnableToReachServer", "تعذر الوصول إلى الخادم، يرجى التأكد من اتصال جهازك بالشبكة"),

            En(Constants.Client, "Error_LoginSessionExpired", "Your session has expired, please sign-in again"),
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

            En(Constants.Client, "Error_CannotModifyInactiveItemPleaseActivate", "An inactive item cannot be edited, please activate it first"),
            Ar(Constants.Client, "Error_CannotModifyInactiveItemPleaseActivate", "لا يمكن تعديل بيان غير منشط، يرجى تنشيط البيان أولا"),

            En(Constants.Client, "Error_UnauthorizedForCompany", "Your account is no longer a member of this company"),
            Ar(Constants.Client, "Error_UnauthorizedForCompany", "حسابك على النظام لم يعد معرفا كعضو في هذه الشركة"),

            En(Constants.Client, "Error_LoadingCompanySettings", "Error loading company settings"),
            Ar(Constants.Client, "Error_LoadingCompanySettings", "حدث خطأ أثناء تحميل إعدادات الشركة"),

            En(Constants.Client, "Error_LoadingGlobalSettings", "Error loading system settings"),
            Ar(Constants.Client, "Error_LoadingGlobalSettings", "حدث خطأ أثناء تحميل إعدادات النظام"),

            En(Constants.Client, "Error_ImageExceedsTheMaximumSizeOfX", "The file you selected exceeds the maximum allowed size of {{maxSize}} MB"),
            Ar(Constants.Client, "Error_ImageExceedsTheMaximumSizeOfX", "حجم الملف الذي اخترته يتجاوز الحد الأقصى المسموح به وهو {{maxSize}} ميجابايت"),

            En(Constants.Client, "Error_UnableToValidateYourCredentials", "Could not validate your credentials, this is likely due to misconfigured system"),
            Ar(Constants.Client, "Error_UnableToValidateYourCredentials", "تعذر التأكد من صحة هويتك، السبب غالبا هو أن إعدادات النظام غير سليمة"),

            
            // Labels
            En(Constants.Shared, "AppName", "BSharp"),
            Ar(Constants.Shared, "AppName", "بيشارپ"),

            En(Constants.Shared, "Home", "Home"),
            Ar(Constants.Shared, "Home", "الرئيسية"),

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

            En(Constants.Shared, "Agent", "Agent"),
            Ar(Constants.Shared, "Agent", "ذمة"),

            En(Constants.Shared, "Agents", "Agents"),
            Ar(Constants.Shared, "Agents", "ذمم"),

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

            En(Constants.Shared, "Agent_TaxIdentificationNumber", "Tax ID Number"),
            Ar(Constants.Shared, "Agent_TaxIdentificationNumber", "رقم السجل الضريبي"),

            En(Constants.Shared, "Agent_Title", "Title"),
            Ar(Constants.Shared, "Agent_Title", "اللقب"),

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

            En(Constants.Shared, "User_New", "Invited"),
            Ar(Constants.Shared, "User_New", "مدعو"),

            En(Constants.Shared, "User_Confirmed", "Confirmed"),
            Ar(Constants.Shared, "User_Confirmed", "مؤكد"),

            En(Constants.Shared, "RoleMembership_User", "User"),
            Ar(Constants.Shared, "RoleMembership_User", "المستخدم"),

            En(Constants.Shared, "RoleMembership_Role", "Role"),
            Ar(Constants.Shared, "RoleMembership_Role", "الدور"),

            En(Constants.Shared, "Settings", "Settings"),
            Ar(Constants.Shared, "Settings", "إعدادات"),

            En(Constants.Shared, "State", "State"),
            Ar(Constants.Shared, "State", "الحالة"),

            En(Constants.Shared, "Settings_ShortCompanyName", "Short Company Name"),
            Ar(Constants.Shared, "Settings_ShortCompanyName", "اسم المؤسسة"),
            En(Constants.Client, "Settings_ShortCompanyNameDescription", "The company name that will appear to users on the screens"),
            Ar(Constants.Client, "Settings_ShortCompanyNameDescription", "اسم المؤسسة الذي يظهر للمستخدمين على الشاشات"),

            En(Constants.Shared, "Settings_PrimaryLanguage", "Primary Language"),
            Ar(Constants.Shared, "Settings_PrimaryLanguage", "اللغة الرئيسية"),
            En(Constants.Client, "Settings_PrimaryLanguageDescription", "Data entry is required using this language"),
            Ar(Constants.Client, "Settings_PrimaryLanguageDescription", "إدخال البيانات إلزامي بهذه اللغة"),

            En(Constants.Shared, "Settings_PrimaryLanguageSymbol", "Primary Language Symbol"),
            Ar(Constants.Shared, "Settings_PrimaryLanguageSymbol", "رمز اللغة الرئيسية"),
            En(Constants.Client, "Settings_LanguageSymbolDescription", "This symbol distinguishes the language of bilingual fields"),
            Ar(Constants.Client, "Settings_LanguageSymbolDescription", "الرمز الذي يميز لغة الحقول المزدوجة اللغة"),

            En(Constants.Client, "Settings_SecondaryLanguage", "Secondary Language"),
            Ar(Constants.Shared, "Settings_SecondaryLanguage", "اللغة الثانوية"),
            En(Constants.Shared, "Settings_SecondaryLanguageDescription", "Allows users to enter data and view reports in 2 languages"),
            Ar(Constants.Client, "Settings_SecondaryLanguageDescription", "تتيح للمستخدمين إدخال البيانات واستعراض التقارير بلغتين"),

            En(Constants.Shared, "Settings_SecondaryLanguageSymbol", "Secondary Language Symbol"),
            Ar(Constants.Shared, "Settings_SecondaryLanguageSymbol", "رمز اللغة الثانوية"),

            En(Constants.Shared, "Settings_BrandColor", "Brand Color"),
            Ar(Constants.Shared, "Settings_BrandColor", "لون العلامة التجارية"),
            En(Constants.Client, "Settings_BrandColorDescription", "The color of the navigation bar at the top of the user interface"),
            Ar(Constants.Client, "Settings_BrandColorDescription", "لون شريط التصفح في أعلى واجهة المستخدم"),

            En(Constants.Shared, "Settings_ProvisionedAt", "Provisioned At"),
            Ar(Constants.Shared, "Settings_ProvisionedAt", "زمن الإنشاء"),

            En(Constants.Shared, "Memo", "Memo"),
            Ar(Constants.Shared, "Memo", "ملاحظات"),

            En(Constants.Shared, "Name", "Name"),
            Ar(Constants.Shared, "Name", "الاسم"),

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

            En(Constants.Client, "WithPicture", "With Picture"),
            Ar(Constants.Client, "WithPicture", "مع صورة"),

            En(Constants.Client, "WithoutPicture", "Without Picture"),
            Ar(Constants.Client, "WithoutPicture", "بدون صورة"),

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

            En(Constants.Client, "MyCompanies", "My Companies"),
            Ar(Constants.Client, "MyCompanies", "شركاتي"),

            En(Constants.Client, "ChangeCompany", "Change Company"),
            Ar(Constants.Client, "ChangeCompany", "تغيير الشركة"),

            En(Constants.Client, "GoBack", "Go Back"),
            Ar(Constants.Client, "GoBack", "رجوع"),

            En(Constants.Client, "TryAgain", "Try Again"),
            Ar(Constants.Client, "TryAgain", "حاول مجددا"),

            En(Constants.Client, "RequiredField", "Required Field"),
            Ar(Constants.Client, "RequiredField", "حقل مطلوب"),

            En(Constants.Client, "InvalidDate", "Invalid Date"),
            Ar(Constants.Client, "InvalidDate", "تاريخ غير سليم"),

            En(Constants.Client, "InvalidEmail", "Invalid Email"),
            Ar(Constants.Client, "InvalidEmail", "عنوان بريد غير سليم"),

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

            En(Constants.Shared, "SignOut", "Sign Out"),
            Ar(Constants.Shared, "SignOut", "تسجيل خروج"),

            En(Constants.Shared, "SignIn", "Sign In"),
            Ar(Constants.Shared, "SignIn", "تسجيل دخول"),

            En(Constants.Client, "My0Account", "My {{placeholder}} Account"),
            Ar(Constants.Client, "My0Account", "حسابي على {{placeholder}}"),

            En(Constants.Client, "WelcomePage", "Welcome Page"),
            Ar(Constants.Client, "WelcomePage", "الصفحة الرئيسية"),

            En(Constants.Client, "NoItemsFound", "No Items Found."),
            Ar(Constants.Client, "NoItemsFound", "لا يوجد بيانات"),

            En(Constants.Client, "CreatedByMe", "Created By Me"),
            Ar(Constants.Client, "CreatedByMe", "منشأ من قبلي"),

            En(Constants.Client, "ImportAFile", "Import a File"),
            Ar(Constants.Client, "ImportAFile", "استيراد ملف"),

            En(Constants.Shared, "Cancel", "Cancel"),
            Ar(Constants.Shared, "Cancel", "إلغاء"),

            En(Constants.Shared, "Save", "Save"),
            Ar(Constants.Shared, "Save", "حفظ"),

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

            En(Constants.Client, "GeneralSettings", "General Settings"),
            Ar(Constants.Client, "GeneralSettings", "إعدادات عامة"),

            En(Constants.Client, "Branding", "Branding"),
            Ar(Constants.Client, "Branding", "علامة تجارية"),

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
            Ar(Constants.Client, "ShortDay1", "ن"),

            En(Constants.Client, "ShortDay2", "Tu"),
            Ar(Constants.Client, "ShortDay2", "ث"),

            En(Constants.Client, "ShortDay3", "We"),
            Ar(Constants.Client, "ShortDay3", "ر"),

            En(Constants.Client, "ShortDay4", "Th"),
            Ar(Constants.Client, "ShortDay4", "خ"),

            En(Constants.Client, "ShortDay5", "Fr"),
            Ar(Constants.Client, "ShortDay5", "ج"),

            En(Constants.Client, "ShortDay6", "Sa"),
            Ar(Constants.Client, "ShortDay6", "س"),

            En(Constants.Client, "ShortDay7", "Su"),
            Ar(Constants.Client, "ShortDay7", "ح"),

            En(Constants.Client, "NewVersionIsAvailable", "A new version is available!"),
            Ar(Constants.Client, "NewVersionIsAvailable", "هنالك إصدار جديد!"),

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

            En(Constants.Client, "CreateOptions", "Create Options"),
            Ar(Constants.Client, "CreateOptions", "خيارات الإنشاء"),

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

            En(Constants.Client, "CreateOptionsMessage", "What type of record would you like to create?"),
            Ar(Constants.Client, "CreateOptionsMessage", "ما نوع البيان الذي تود إنشاءه؟"),

            En(Constants.Client, "Activate", "Activate"),
            Ar(Constants.Client, "Activate", "تنشيط"),

            En(Constants.Client, "Deactivate", "Deactivate"),
            Ar(Constants.Client, "Deactivate", "إيقاف النشاط"),

            En(Constants.Client, "Error", "Error"),
            Ar(Constants.Client, "Error", "حدث خطأ"),

            En(Constants.Client, "Success", "Success"),
            Ar(Constants.Client, "Success", "تم بنجاح"),

            En(Constants.Client, "Unauthorized", "Unauthorized"),
            Ar(Constants.Client, "Unauthorized", "غير مصرح"),

            En(Constants.Client, "Dismiss", "Dismiss"),
            Ar(Constants.Client, "Dismiss", "إزالة"),

            En(Constants.Client, "ClearFilter", "Clear Filter"),
            Ar(Constants.Client, "ClearFilter", "إزالة التصفية"),

            En(Constants.Client, "ResendInvitationEmail", "Re-Send Invitation"),
            Ar(Constants.Client, "ResendInvitationEmail", "إعادة إرسال الدعوة"),

            En(Constants.Client, "InvitationEmailSent", "The invitation email was sent again"),
            Ar(Constants.Client, "InvitationEmailSent", "تم إرسال الدعوة مرة أخرى بالبريد الاإلكتروني"),

            

            En(Constants.Server, "ActionDidNotPassValidation", "The action did not pass validation, see the highlighted rows for details"),
            Ar(Constants.Server, "ActionDidNotPassValidation", "الأمر المنفذ لم يستوف شروط الصحة، راجع الأسطر المؤشر عليها لمزيد من التفاصيل"),

            En(Constants.Server, "InvitationEmailSubject0", "Invitation to {0}"),
            Ar(Constants.Server, "InvitationEmailSubject0", "دعوة للانضمام إلى {0}"),

            En(Constants.Server, "InvitationEmailGreeting0", "Hello {0}!"),
            Ar(Constants.Server, "InvitationEmailGreeting0", "مرحبا {0}!"),

            En(Constants.Server, "InvitationEmailBody012", "{0} has invited you to join {1} ERP. Click on the button below to accept the invitation and create your account. This invitation is only valid for {2} days."),
            Ar(Constants.Server, "InvitationEmailBody012", "لديك دعوة من {0} للانضمام إلى عائلة {1}، إضغط على الزر أدناه لقبول هذه الدعوة وإنشاء حسابك، الدعوة صالحة لمدة {2} أيام فقط."),

            En(Constants.Server, "InvitationEmailButtonLabel", "Accept Invitation"),
            Ar(Constants.Server, "InvitationEmailButtonLabel", "قبول الدعوة"),

            En(Constants.Server, "InvitationEmailConclusion", "Yours,"),
            Ar(Constants.Server, "InvitationEmailConclusion", "مبارك،"),

            En(Constants.Server, "InvitationEmailSignature0", "The {0} team"),
            Ar(Constants.Server, "InvitationEmailSignature0", "فريق {0}"),










            // Identity Labels            
            
            En(Constants.Server, "RememberMe", "Remember me?"),
            Ar(Constants.Server, "RememberMe", "إبق متصلا؟"),

            En(Constants.Server, "Email", "Email"),
            Ar(Constants.Server, "Email", "البريد الإلكتروني"),

            En(Constants.Server, "Password", "Password"),
            Ar(Constants.Server, "Password", "كلمة المرور"),

            En(Constants.Server, "ConfirmPassword", "Confirm Password"),
            Ar(Constants.Server, "ConfirmPassword", "تأكيد كلمة المرور"),

            En(Constants.Server, "Error_ThePasswordAndConfirmationPasswordDoNotMatch", "The password and confirmation password do not match."),
            Ar(Constants.Server, "Error_ThePasswordAndConfirmationPasswordDoNotMatch", "كلمة المرور وتأكيدها غير متطابقين"),

            En(Constants.Server, "ForgotYourPassword", "Forgot your password?"),
            Ar(Constants.Server, "ForgotYourPassword", "نسيت كلمة المرور؟"),

            En(Constants.Server, "ForgotPasswordConfirmation", "Forgot password confirmation"),
            Ar(Constants.Server, "ForgotPasswordConfirmation", "تأكيد فقد كلمة المرور"),

            En(Constants.Server, "ForgotPasswordConfirmationMessage", "Please check your email to reset your password."),
            Ar(Constants.Server, "ForgotPasswordConfirmationMessage", "راجع بريدك الإكتروني لتحديد كلمة مرور جديدة"),

            En(Constants.Server, "ResetPasswordConfirmation", "Reset password confirmation"),
            Ar(Constants.Server, "ResetPasswordConfirmation", "تأكيد إعادة تعيين كلمة المرور"),

            En(Constants.Server, "ResetPasswordConfirmationMessage", "Your password has been reset, click below to sign in"),
            Ar(Constants.Server, "ResetPasswordConfirmationMessage", "أعيد تعيين كلمة مرورك بنجاح، يمكنك تسجيل دخولك بالضغط على الزر أدناه"),

            En(Constants.Server, "ClickHere", "Click here"),
            Ar(Constants.Server, "ClickHere", "إضغط هنا"),

            En(Constants.Server, "ClickHereLower", "click here"),
            Ar(Constants.Server, "ClickHereLower", "إضغط هنا"),

            En(Constants.Server, "SignInWith0", "Sign in with {0}"),
            Ar(Constants.Server, "SignInWith0", "تسجيل دخول بواسطة {0}"),

            En(Constants.Server, "Google", "Google"),
            Ar(Constants.Server, "Google", "جوجل"),

            En(Constants.Server, "Microsoft", "Microsoft"),
            Ar(Constants.Server, "Microsoft", "مايكروسوفت"),

            En(Constants.Server, "Facebook", "Facebook"),
            Ar(Constants.Server, "Facebook", "الفيسبوك"),

            En(Constants.Server, "Twitter", "Twitter"),
            Ar(Constants.Server, "Twitter", "تويتر"),

            En(Constants.Server, "CopyrightNotice0", "Copyright © {0} Banan IT, Ltd. All rights reserved."),
            Ar(Constants.Server, "CopyrightNotice0", "بنان لتقنية المعلومات © {0} جميع الحقوق محفوظة"),

            En(Constants.Server, "PrivacyPolicy", "Privacy Policy"),
            Ar(Constants.Server, "PrivacyPolicy", "سياسة الخصوصية"),

            En(Constants.Server, "TermsOfService", "Terms of Service"),
            Ar(Constants.Server, "TermsOfService", "شروط الخدمة"),

            En(Constants.Server, "Or", "Or"),
            Ar(Constants.Server, "Or", "أو"),

            En(Constants.Server, "EnterYourEmail", "Enter your email"),
            Ar(Constants.Server, "EnterYourEmail", "أدخل عنوان بريدك الإلكتروني"),

            En(Constants.Server, "ResetMyPassword", "Reset My Password"),
            Ar(Constants.Server, "ResetMyPassword", "إعادة تعيين كلمة المرور"),

            En(Constants.Server, "CreateAPassword", "Create a Password"),
            Ar(Constants.Server, "CreateAPassword", "تحديد كلمة المرور"),

            En(Constants.Server, "ResetYourPassword", "Reset your password"),
            Ar(Constants.Server, "ResetYourPassword", "إعادة تعيين كلمة المرور"),

            En(Constants.Server, "ResetPasswordEmailMessage", "Please reset your password by <a href='{0}'>clicking here</a>."),
            Ar(Constants.Server, "ResetPasswordEmailMessage", "حدد كلمة مرور جديدة عبر <a href='{0}'>هذه الوصلة</a>."),

            En(Constants.Server, "AccessDenied", "Access Denied"),
            Ar(Constants.Server, "AccessDenied", "غير مسموح بالدخول"),

            En(Constants.Server, "AccessDeniedMessage", "You do not have access to this resource."),
            Ar(Constants.Server, "AccessDeniedMessage", "ليس لديك حق الوصول إلى هذا المورد."),

            En(Constants.Server, "LockedOut", "Locked Out"),
            Ar(Constants.Server, "LockedOut", "حساب معلق"),

            En(Constants.Server, "LockedOutMessage", "This account has been temporarily locked out, please try again later."),
            Ar(Constants.Server, "LockedOutMessage", ".هذا الحساب تم تعليقه مؤقتا، يرجى المحاولة مرة أخرى لاحقا"),

            En(Constants.Server, "PleaseConfirm", "Please Confirm"),
            Ar(Constants.Server, "PleaseConfirm", "يرجى التأكيد"),

            En(Constants.Server, "SignOutMessage0", "Would you like to sign out of {0}?"),
            Ar(Constants.Server, "SignOutMessage0", "هل تود تسجيل خروجك من {0}؟"),

            En(Constants.Server, "SignOutConfirmation", "Sign out confirmation"),
            Ar(Constants.Server, "SignOutConfirmation", "تأكيد تسجيل الخروج"),

            En(Constants.Server, "SignOutConfirmationMessage", "You are now signed out"),
            Ar(Constants.Server, "SignOutConfirmationMessage", "تم تسجيل خروجك"),

            En(Constants.Server, "ConfirmEmail", "Confirm Email"),
            Ar(Constants.Server, "ConfirmEmail", "تأكيد البريد الإلكتروني"),

            En(Constants.Server, "EmailConfirmationMessage", "Thank you for confirming your email"),
            Ar(Constants.Server, "EmailConfirmationMessage", "شكرا على تأكيد عنوان بريدك الإلكتروني"),

            En(Constants.Server, "TwoFactorAuthentication", "Two-Factor Authentication"),
            Ar(Constants.Server, "TwoFactorAuthentication", "توثيق مزدوج العوامل"),

            En(Constants.Server, "TwoFactorAuthenticationInstructions", "Your account is protected with an authenticator app. Enter your authenticator code below"),
            Ar(Constants.Server, "TwoFactorAuthenticationInstructions", "حسابك محمي بتطبيق (authenticator)، أدخل الكود الذي يعرضه التطبيق في الحقل أدناه"),

            En(Constants.Server, "AuthenticatorCode", "Authenticator Code"),
            Ar(Constants.Server, "AuthenticatorCode", "الكود من تطبيق (authenticator)"),

            En(Constants.Server, "InvalidAuthenticatorCode", "Invalid authenticator code."),
            Ar(Constants.Server, "InvalidAuthenticatorCode", "الكود غير سليم."),

            En(Constants.Server, "RememberThisDevice", "Remember this browser"),
            Ar(Constants.Server, "RememberThisDevice", "تذكر هذا المتصفح"),

            En(Constants.Server, "TwoFactorAuthenticationRecoveryInstructions", "Don't have access to your authenticator device? To log in with a recovery code"),
            Ar(Constants.Server, "TwoFactorAuthenticationRecoveryInstructions", "تطبيق (authenticator) ليس في متناول يدك؟ لتسجيل دخولك باستخدام كود استرداد "),

            En(Constants.Server, "RecoveryCodeVerification", "Recovery Code Verification"),
            Ar(Constants.Server, "RecoveryCodeVerification", "التحقق من كود الاسترداد"),

            En(Constants.Server, "RecoveryCode", "Recovery Code"),
            Ar(Constants.Server, "RecoveryCode", "كود الاسترداد"),

            En(Constants.Server, "RecoveryCodeVerificationMessage", "You have requested to log in with a recovery code. This session will not be remembered until you provide an authenticator app code or disable 2-factor authentication and sign in again."),
            Ar(Constants.Server, "RecoveryCodeVerificationMessage", "إذا كنت تود تسجيل دخولك باستخدام كود استرداد فإن دخولك لن يستمر لفترة ممتدة ما لم تأت بكود من تطبيق (authenticator) أو تقوم بتعطيل ميزة التوثيق المزدوج العوامل ومن ثم تعيد تسجيل دخولك مجددا."),

            En(Constants.Server, "InvalidRecoveryCode", "Invalid recovery code."),
            Ar(Constants.Server, "InvalidRecoveryCode", "كود الاسترداد غير سليم."),

            En(Constants.Server, "Error_ErrorFromExternalProvider0", "Error from external provider: {0}"),
            Ar(Constants.Server, "Error_ErrorFromExternalProvider0", "رسالة خطأ من مسجل الدخول الخارجي"),

            En(Constants.Server, "Error_LoadingExternalLoginInformation", "Error loading external login information."),
            Ar(Constants.Server, "Error_LoadingExternalLoginInformation", "حدث خطأ لدى تحميل معلومات مسجل الدخول الخارجي"),

            En(Constants.Server, "Error_EmailNotProvidedByExternalSignIn", "The email was not provided by the external sign-in"),
            Ar(Constants.Server, "Error_EmailNotProvidedByExternalSignIn", "مسجل الدخول الخارجي لم يفصح عن عنوان بريدك الإلكتروني"),

            En(Constants.Server, "Error_AUserWithEmail0CouldNotBeFound", "A user with email '{0}' could not be found"),
            Ar(Constants.Server, "Error_AUserWithEmail0CouldNotBeFound", "لم يتم العثور على حساب مستخدم عنوان بريده الإكتروني ({0})"),

            En(Constants.Server, "Error_AddingLoginForUserWithEmail0", "Unexpected error occurred adding external login for user with email '{0}'."),
            Ar(Constants.Server, "Error_AddingLoginForUserWithEmail0", "حدث خطأ غير متوقع عند تعريف مسجل الدخول الخارجي للمستخدم ذي عنوان البريد الإكتروني ({0})"),

            En(Constants.Server, "Error_TryingToSignYouInWithExternalProvider", "An unknown error occurred while trying to sign you in using the external provider"),
            Ar(Constants.Server, "Error_TryingToSignYouInWithExternalProvider", "حدث خطأ مجهول لدى تسجيل دخولك عن طريق مسجل الدخول الخارجي"),

            En(Constants.Server, "Error_ConfirmingYourEmail", "An error occurred while confirming your email"),
            Ar(Constants.Server, "Error_ConfirmingYourEmail", "حدث خطأ عند محاولة تأكيد عنوان بريدك الإلكتروني"),

            En(Constants.Server, "Error_InvalidLoginAttempt", "Invalid sign-in attempt. If you haven't confirmed your email yet please check your email inbox"),
            Ar(Constants.Server, "Error_InvalidLoginAttempt", "تسجيل الدخول لم يكلل بالنجاح، إذا لم تٌأكّد عنوان بريدك الإلكتروني بعد قم بمراجعة صندوق الوارد"),

            En(Constants.Shared, "GoTo0", "Go to {0}"),
            Ar(Constants.Shared, "GoTo0", "ذهاب إلى {0}"),















            // Identity Management
            
            En(Constants.Server, "Menu_Profile", "Profile"),
            Ar(Constants.Server, "Menu_Profile", "الملف الشخصي"),

            En(Constants.Server, "Menu_Password", "Password"),
            Ar(Constants.Server, "Menu_Password", "كلمة المرور"),

            En(Constants.Server, "Menu_ExternalSignIns", "External Sign-ins"),
            Ar(Constants.Server, "Menu_ExternalSignIns", "تسجيل الدخول الخارجي"),

            En(Constants.Server, "Menu_TwoFactorAuthentication", "2FA"),
            Ar(Constants.Server, "Menu_TwoFactorAuthentication", "التوثيق مزدوج العوامل"),

            En(Constants.Server, "PhoneNumber", "Phone Number"),
            Ar(Constants.Server, "PhoneNumber", "رقم الهاتف"),

            En(Constants.Server, "UserName", "User Name"),
            Ar(Constants.Server, "UserName", "اسم المستخدم"),

            En(Constants.Server, "YourProfileHasBeenUpdated", "Your profile has been updated."),
            Ar(Constants.Server, "YourProfileHasBeenUpdated", "تم تحديث حسابك."),

            En(Constants.Server, "ChangePassword", "Change Password"),
            Ar(Constants.Server, "ChangePassword", "تغير كلمة المرور"),

            En(Constants.Server, "CurrentPassword", "Current Password"),
            Ar(Constants.Server, "CurrentPassword", "كلمة المرور الحالية"),

            En(Constants.Server, "NewPassword", "New Password"),
            Ar(Constants.Server, "NewPassword", "كلمة المرور الجديدة"),

            En(Constants.Server, "ConfirmNewPassword", "Confirm New Password"),
            Ar(Constants.Server, "ConfirmNewPassword", "تأكيد كلمة المرور الجديدة"),

            En(Constants.Server, "YourPasswordHasBeenChanged", "Your password has been changed."),
            Ar(Constants.Server, "YourPasswordHasBeenChanged", "تم تحديث كلمة مرورك."),

            En(Constants.Server, "RegisteredExternalSignIns", "Your External Sign-ins"),
            Ar(Constants.Server, "RegisteredExternalSignIns", "مسجلو دخولك الخارجيون"),
            
            En(Constants.Server, "AddAnExternalSignIn", "Add an External Sign-in"),
            Ar(Constants.Server, "AddAnExternalSignIn", "أضف مسجل دخول خارجي"),

            En(Constants.Server, "Remove0SignInFromYourAccount", "Remove {0} sign in from your account"),
            Ar(Constants.Server, "Remove0SignInFromYourAccount", "إزالة {0} كمسجل دخول خارجي إلى حسابك"),

            En(Constants.Server, "SignInUsingYour0Account", "Sign in using your {0} account"),
            Ar(Constants.Server, "SignInUsingYour0Account", "سجل دخولك عن طريق حسابك عند {0}"),

            En(Constants.Server, "TheExternalSignInWasAdded", "The external sign-in was added."),
            Ar(Constants.Server, "TheExternalSignInWasAdded", "تم إضافة مسجل الدخول الخارجي."),

            En(Constants.Server, "TheExternalSignInWasRemoved", "The external sign-in was removed."),
            Ar(Constants.Server, "TheExternalSignInWasRemoved", "تم إزالة مسجل الدخول الخارجي."),

            En(Constants.Server, "SetPassword", "Set Password"),
            Ar(Constants.Server, "SetPassword", "تحديد كلمة المرور"),

            En(Constants.Server, "SetPasswordMessage", "You do not have a local username/password for this application. Add a local password so you can log in without an external sign-in provider."),
            Ar(Constants.Server, "SetPasswordMessage", "ليس لديك اسم مستخدم وكلمة مرور خاصين بهذا التطبيق، أضف كلمة مرور حتى يتسنى لك تسجيل دخولك من دون مسجل دخول خارجي."),

            En(Constants.Server, "YourPasswordHasBeenSet", "Your password has been set."),
            Ar(Constants.Server, "YourPasswordHasBeenSet", "تم تحديد كلمة مرورك."),

            En(Constants.Server, "ConfigureAuthenticatorApp", "Configure Authenticator App"),
            Ar(Constants.Server, "ConfigureAuthenticatorApp", "ضبط تطبيق (Authenticator)"),

            En(Constants.Server, "ConfigureAuthenticatorIntro", "To use an authenticator app go through the following steps:"),
            Ar(Constants.Server, "ConfigureAuthenticatorIntro", "لاستخدام تطبيق (authenticator) قم باتباع الخطوات التالية:"),

            En(Constants.Server, "ConfigureAuthenticatorStep1", "Download a two-factor authenticator app like Google Authenticator:"),
            Ar(Constants.Server, "ConfigureAuthenticatorStep1", "حمل تطبيقا للتوثيق المزدوج العوامل، مثلا (Google Authenticator):"),

            En(Constants.Server, "ConfigureAuthenticatorStep2", "Scan the QR Code or enter the following key into your two factor authenticator app. Spaces and casing do not matter:"),
            Ar(Constants.Server, "ConfigureAuthenticatorStep2", "قم بمسح الباركود أدناه أو قم بإدخال الكود التالي في تطبيق (authenticator)، حجم الرموز لا يؤثر وكذلك المسافات بينها:"),

            En(Constants.Server, "ConfigureAuthenticatorStep3", "Once you have scanned the QR code or input the key above, your authenticator app will provide you with a secret code. Enter the code in the confirmation box below."),
            Ar(Constants.Server, "ConfigureAuthenticatorStep3", "بعد مسح الباركود أو إدخال الكود، سيعرض لك التطبيق رقما سريا للتحقق، قم بإدخاله في الحقل أدناه."),
            
            En(Constants.Server, "Android", "Android"),
            Ar(Constants.Server, "Android", "الآندرويد"),

            En(Constants.Server, "iOS", "iOS"),
            Ar(Constants.Server, "iOS", "الآيفون"),

            En(Constants.Server, "Verify", "Verify"),
            Ar(Constants.Server, "Verify", "تحقق"),

            En(Constants.Server, "VerificationCode", "Verification Code"),
            Ar(Constants.Server, "VerificationCode", "رقم التحقق"),

            En(Constants.Server, "VerificationCodeInvalid", "Verification code is invalid."),
            Ar(Constants.Server, "VerificationCodeInvalid", "رقم التحقق غير صحيح"),

            En(Constants.Server, "YourAuthenticatorAppHasBeenVerified", "Your authenticator app has been verified."),
            Ar(Constants.Server, "YourAuthenticatorAppHasBeenVerified", "تم التحقق من تطبيق (authenticator) خاصتك"),

            En(Constants.Server, "ResetAuthenticatorKey", "Reset Authenticator Key"),
            Ar(Constants.Server, "ResetAuthenticatorKey", "إعادة تعيين مفتاح الـ(Authenticator)"),

            En(Constants.Server, "ResetAuthenticatorKeyWarningTitle", "If you reset your authenticator key your authenticator app will not work until you reconfigure it."),
            Ar(Constants.Server, "ResetAuthenticatorKeyWarningTitle", "إذا أعدت تعيين هذا المفتاح، لن يعمل تطبيق (authenticator) الخاص بك ما لم تقم بإعادة ضبطه مجددا"),

            En(Constants.Server, "ResetAuthenticatorKeyWarningBody", "This process disables 2FA until you verify your authenticator app. If you do not complete your authenticator app configuration you may lose access to your account."),
            Ar(Constants.Server, "ResetAuthenticatorKeyWarningBody", "هذه العملية تقوم متعطيل التحقيق مزدوج العوامل حتى تقوم بضبط تطبيق (authenticator) مرة أخرى، إذا لم تقم بعملية الضبط على الفور قد تفقد إمكانية الدخول باستخدام هذا الحساب"),

            En(Constants.Server, "YourAuthenticatorAppKeyHasBeenReset", "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key."),
            Ar(Constants.Server, "YourAuthenticatorAppKeyHasBeenReset", "تم إعادة تعيين مفتاح تطبيق (authenticator)، عليك بإعادة ضبط التطبيق حالا باستخدام المفتاح الجديد"),

            En(Constants.Server, "DisableTwoFactorAuthentication", "Disable Two-Factor Authentication (2FA)"),
            Ar(Constants.Server, "DisableTwoFactorAuthentication", "تعطيل التوثيق مزدوج العوامل"),

            En(Constants.Server, "DisableTwoFactorAuthenticationTitle", "This action only disables 2FA."),
            Ar(Constants.Server, "DisableTwoFactorAuthenticationTitle", "هذا الأمر يقوم بتعطيل التوثيق مزدوج العوامل"),

            En(Constants.Server, "DisableTwoFactorAuthenticationBody", "Disabling 2FA does not change the keys used in authenticator apps. If you wish to change the key used in an authenticator app"),
            Ar(Constants.Server, "DisableTwoFactorAuthenticationBody", "تعطيل التوثيق مزدوج العوامل لا يقوم بإعادة تعيين مفتاح الـ(authenticator) خاصتك، لإعادة تعيين المفتاح"),

            En(Constants.Server, "TwoFactorAuthenticatorHasBeenDisabled", "2FA has been disabled. You can re-enable 2FA by setting up an authenticator app"),
            Ar(Constants.Server, "TwoFactorAuthenticatorHasBeenDisabled", "تم تعطيل التوثيق مزدوج العوامل، لإعادة تشغيله ثقم بضبط تطبيق (authenticator)"),

            En(Constants.Server, "GenerateTwoFactorAuthenticationRecoveryCodes", "Generate Two-Factor Authentication (2FA) Recovery Codes"),
            Ar(Constants.Server, "GenerateTwoFactorAuthenticationRecoveryCodes", "توليد أكواد استرداد للتوثيق مزدوج العوامل"),

            En(Constants.Server, "GenerateRecoveryCodes", "Generate Recovery Codes"),
            Ar(Constants.Server, "GenerateRecoveryCodes", "توليد أكواد الاسترداد"),

            En(Constants.Server, "RecoveryCodesWarningTitle", "Put these codes in a safe place."),
            Ar(Constants.Server, "RecoveryCodesWarningTitle", "احتفظ بهذه الأكواد في مكان آمن"),

            En(Constants.Server, "RecoveryCodesWarningBody", "If you lose your authenticator app and you don't have the recovery codes you will lose access to your account."),
            Ar(Constants.Server, "RecoveryCodesWarningBody", "إذا فقدت جهازك ذا تطبيق (authenticator) وفقدت معه هذه الأكواد لن تتمكن من تسجيل دخولك على هذا الحساب مرة أخرى."),

            En(Constants.Server, "YouHaveGeneratedNewRecoveryCodes", "You have generated new recovery codes."),
            Ar(Constants.Server, "YouHaveGeneratedNewRecoveryCodes", "لقد قمت بتوليد أكواد استرداد جديدة"),

            En(Constants.Server, "RecoveryCodes", "Recovery Codes"),
            Ar(Constants.Server, "RecoveryCodes", "أكواد الاسترداد"),

            En(Constants.Server, "Page_TwoFactorAuthentication", "Two-Factor Authentication (2FA)"),
            Ar(Constants.Server, "Page_TwoFactorAuthentication", "التوثيق مزدوج العوامل"),

            En(Constants.Server, "RecoveryCodesLeft0", "Remaining recovery codes: {0}"),
            Ar(Constants.Server, "RecoveryCodesLeft0", "عدد أكواد الاسترداد المتبقية: {0}"),

            En(Constants.Server, "YouShouldGenerateNewRecoveryCodes", "You should generate a new set of recovery codes."),
            Ar(Constants.Server, "YouShouldGenerateNewRecoveryCodes", "عليك بتوليد مجموعة جديدة من أكواد الاسترداد،"),

            En(Constants.Server, "AuthenticatorApp", "Authenticator App"),
            Ar(Constants.Server, "AuthenticatorApp", "تطبيق (Authenticator)"),

            En(Constants.Server, "ForgetThisBrowser", "Forget This Browser"),
            Ar(Constants.Server, "ForgetThisBrowser", "تناسى المتصفح الحالي"),

            En(Constants.Server, "TheCurrentBrowserHasBeenForgotten", "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code."),
            Ar(Constants.Server, "TheCurrentBrowserHasBeenForgotten", "تم تناسي المتصفح الحالي، عند تسجيل دخولك من هذا المتصفح في المرة المقبلة سيطالبك النظام بالكود من تطبيق (authenticator)."),

            En(Constants.Server, "Button_DisableTwoFactorAuthentication", "Disable 2FA"),
            Ar(Constants.Server, "Button_DisableTwoFactorAuthentication", "تعطيل التوثيق مزدوج العوامل"),

            En(Constants.Server, "ResetRecoveryCodes", "Reset Recovery Codes"),
            Ar(Constants.Server, "ResetRecoveryCodes", "إعادة توليد أكواد الاسترداد"),





            En(Constants.Server, nameof(IdentityErrorDescriber.DefaultError), "An unknown failure has occurred."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.DefaultError), "حدث خطأ غير معروف."),

            En(Constants.Server, nameof(IdentityErrorDescriber.PasswordMismatch), "Incorrect password."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.PasswordMismatch), "كلمة المرور غير صحيحة."),

            En(Constants.Server, nameof(IdentityErrorDescriber.InvalidToken), "Invalid token."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.InvalidToken), "الرمز غير سليم."),

            En(Constants.Server, nameof(IdentityErrorDescriber.LoginAlreadyAssociated), "A user with this login already exists."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.LoginAlreadyAssociated), "مسجل الدخول الخارجي هذا مرتبط بمستخدم حالي."),

            En(Constants.Server, nameof(IdentityErrorDescriber.InvalidUserName), "User name '{0}' is invalid, can only contain letters or digits."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.InvalidUserName), "اسم المستخدم ({0}) غير سليم، يجب أن يحتوي فقط على حروف وأرقام."),

            En(Constants.Server, "Identity_" + nameof(IdentityErrorDescriber.InvalidEmail), "Email '{0}' is invalid."),
            Ar(Constants.Server, "Identity_" + nameof(IdentityErrorDescriber.InvalidEmail), "عنوان البريد الإلكتروني ({0}) غير سليم."),

            En(Constants.Server, nameof(IdentityErrorDescriber.DuplicateUserName), "User name '{0}' is already taken."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.DuplicateUserName), "اسم المستخدم ({0}) محجوز."),

            En(Constants.Server, nameof(IdentityErrorDescriber.DuplicateEmail), "Email '{0}' is already taken."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.DuplicateEmail), "عنوان البريد الإلكتروني ({0}) محجوز."),

            En(Constants.Server, nameof(IdentityErrorDescriber.InvalidRoleName), "Role name '{0}' is invalid."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.InvalidRoleName), "اسم الدور ({0}) غير سليم."),

            En(Constants.Server, nameof(IdentityErrorDescriber.DuplicateRoleName), "Role name '{0}' is already taken."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.DuplicateRoleName), "اسم الدور ({0}) محجوز."),

            En(Constants.Server, nameof(IdentityErrorDescriber.UserAlreadyHasPassword), "User already has a password set."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.UserAlreadyHasPassword), "المستخدم لديه كلمة مرور."),

            En(Constants.Server, nameof(IdentityErrorDescriber.UserLockoutNotEnabled), "Lockout is not enabled for this user."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.UserLockoutNotEnabled), "تعليق الحساب غير متاح لهذا المستخدم"),

            En(Constants.Server, nameof(IdentityErrorDescriber.UserAlreadyInRole), "User already in role '{0}'."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.UserAlreadyInRole), "المستخدم موجود في دور ({0})."),

            En(Constants.Server, nameof(IdentityErrorDescriber.UserNotInRole), "User is not in role '{0}'."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.UserNotInRole), "المستخدم ليس عنده دور ({0})."),

            En(Constants.Server, nameof(IdentityErrorDescriber.PasswordTooShort), "Passwords must be at least {0} characters."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.PasswordTooShort), "يجب ألا يقل طول كلمة المرور عن {0} رموز"),

            En(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric), "Passwords must have at least one non alphanumeric character."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric), "يجب أن تحتوي كلمة المرور على رمز غير أبجدي واحد على الأقل"),

            En(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresDigit), "Passwords must have at least one digit ('0'-'9')."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresDigit), "يجب أن تحتوي كلمة المرور على رقم واحد على الأقل ('0'-'9')"),

            En(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresLower), "Passwords must have at least one lowercase ('a'-'z')."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresLower), "يجب أن تحتوي كلمة المرور على حرف لاتيني صغير واحد على الأقل ('a'-'z')"),

            En(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresUpper), "Passwords must have at least one uppercase ('A'-'Z')."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresUpper), "يجب أن تحتوي كلمة المرور على حرف لاتيني كبير واحد على الأقل ('A'-'Z')"),

            En(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars), "Passwords must use at least {0} different characters."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars), "يجب أن تحتوي كلمة المرور على الأقل على عدد {0} من الحروف المميزة عن بعضها"),

            En(Constants.Server, nameof(IdentityErrorDescriber.RecoveryCodeRedemptionFailed), "Recovery code redemption failed."),
            Ar(Constants.Server, nameof(IdentityErrorDescriber.RecoveryCodeRedemptionFailed), "تحصيل كود الاسترداد لم يكلل بالنجاح"),




            



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
