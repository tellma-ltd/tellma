-- Users
INSERT INTO dbo.Translations([TableName],[SourceEnglishWord],[DestinationCultureId],[DestinationWord],[Form]) VALUES
(N'Users',N'Administrator', N'ar', N'المشرف', N'n'),(N'Users',N'Administrator', N'am', N'አስተዳዳሪ', N'n'),(N'Users',N'Administrator', N'cn', N'管理员', N'n');
-- Roles
INSERT INTO dbo.Translations([TableName],[SourceEnglishWord],[DestinationCultureId],[DestinationWord],[Form]) VALUES
(N'Roles',N'Administrator', N'ar', N'المشرف', N'n'),(N'Roles',N'Administrator', N'am', N'አስተዳዳሪ', N'n'),(N'Roles',N'Administrator', N'cn', N'管理员', N'n'),
(N'Roles',N'Finance Manager', N'ar', N'المدير المالي', N'n'),(N'Roles',N'Finance Manager', N'am', N'የፋይናንስ አስተዳዳሪ', N'n'),(N'Roles',N'Finance Manager', N'cn', N'财务经理', N'n'),
(N'Roles',N'General Manager', N'ar', N'المدير العام', N'n'),(N'Roles',N'General Manager', N'am', N'ሰላም ነው', N'n'),(N'Roles',N'General Manager', N'cn', N'总经理', N'n'),
(N'Roles',N'Reader', N'ar', N'صلاحية قراءة', N'n'),(N'Roles',N'Reader', N'am', N'አንባቢ', N'n'),(N'Roles',N'Reader', N'cn', N'读者', N'n'),
(N'Roles',N'Account Manager', N'ar', N'مدير حساب العملاء', N'n'),(N'Roles',N'Account Manager', N'am', N'የደንበኛ መለያ አቀናባሪ', N'n'),(N'Roles',N'Account Manager', N'cn', N'客户客户经理', N'n'),
(N'Roles',N'Comptroller', N'ar', N'مراقب الحسابات', N'n'),(N'Roles',N'Comptroller', N'am', N'የመለያ ኮምፒተር', N'n'),(N'Roles',N'Comptroller', N'cn', N'帐户主计长', N'n'),
(N'Roles',N'Cashier', N'ar', N'أمين الصندوق', N'n'),(N'Roles',N'Cashier', N'am', N'ገንዘብ ተቀባይ', N'n'),(N'Roles',N'Cashier', N'cn', N'出纳员', N'n'),
(N'Roles',N'Admin. Affairs', N'ar', N'الشؤون الإدارية', N'n'),(N'Roles',N'Admin. Affairs', N'am', N'አስተዳደራዊ ጉዳዮች', N'n'),(N'Roles',N'Admin. Affairs', N'cn', N'政务', N'n'),
(N'Roles',N'Production Manager', N'ar', N'مدير الانتاج', N'n'),(N'Roles',N'Production Manager', N'am', N'የምርት ሥራ አስኪያጅ', N'n'),(N'Roles',N'Production Manager', N'cn', N'产品经理', N'n'),
(N'Roles',N'HR Manager', N'ar', N'مدير الموارد البشرية', N'n'),(N'Roles',N'HR Manager', N'am', N'የሰው ኃይል ሥራ አስኪያጅ', N'n'),(N'Roles',N'HR Manager', N'cn', N'人力资源经理', N'n'),
(N'Roles',N'Sales Manager', N'ar', N'مدير المبيعات', N'n'),(N'Roles',N'Sales Manager', N'am', N'የሽያጭ ሃላፊ', N'n'),(N'Roles',N'Sales Manager', N'cn', N'销售经理', N'n'),
(N'Roles',N'Sales Person', N'ar', N'مندوب مبيعات', N'n'),(N'Roles',N'Sales Person', N'am', N'የሽያጭ ሰው', N'n'),(N'Roles',N'Sales Person', N'cn', N'销售人员', N'n'),
(N'Roles',N'Inventory Custodian', N'ar', N'أمين المخزون', N'n'),(N'Roles',N'Inventory Custodian', N'am', N'ኢን Custስትሜንት ባለሞያ', N'n'),(N'Roles',N'Inventory Custodian', N'cn', N'库存保管人', N'n'),
(N'Roles',N'Public', N'ar', N'صلاحيات عامة', N'n'),(N'Roles',N'Public', N'am', N'ሕዝባዊ', N'n'),(N'Roles',N'Public', N'cn', N'上市', N'n');
-- Lookup Definitions
INSERT INTO dbo.Translations([TableName],[SourceEnglishWord],[DestinationCultureId],[DestinationWord],[Form]) VALUES
(N'LookupDefinitions',N'IT Manufacturer', N'ar', N'الشركة المصنعة', N's'),(N'LookupDefinitions',N'IT Manufacturer', N'am', N'የአይቲ አምራች', N's'),(N'LookupDefinitions',N'IT Manufacturer', N'cn', N'IT制造商', N's'),(N'LookupDefinitions',N'IT Manufacturers', N'ar', N'مصنعي تكنولوجيا المعلومات', N'p'),(N'LookupDefinitions',N'IT Manufacturers', N'am', N'የአይቲ አምራቾች', N'p'),(N'LookupDefinitions',N'IT Manufacturers', N'cn', N'IT厂商', N'p'),
(N'LookupDefinitions',N'Operating System', N'ar', N'نظام التشغيل', N's'),(N'LookupDefinitions',N'Operating System', N'am', N'የአሰራር ሂደት', N's'),(N'LookupDefinitions',N'Operating System', N'cn', N'操作系统', N's'),(N'LookupDefinitions',N'Operating Systems', N'ar', N'أنظمة التشغيل', N'p'),(N'LookupDefinitions',N'Operating Systems', N'am', N'ስርዓተ ክወናዎች', N'p'),(N'LookupDefinitions',N'Operating Systems', N'cn', N'操作系统', N'p'),
(N'LookupDefinitions',N'Body Color', N'ar', N'لون الجسم', N's'),(N'LookupDefinitions',N'Body Color', N'am', N'የሰውነት ቀለም', N's'),(N'LookupDefinitions',N'Body Color', N'cn', N'机身颜色', N's'),(N'LookupDefinitions',N'Body Colors', N'ar', N'ألوان الجسم', N'p'),(N'LookupDefinitions',N'Body Colors', N'am', N'የሰውነት ቀለሞች', N'p'),(N'LookupDefinitions',N'Body Colors', N'cn', N'车身颜色', N'p'),
(N'LookupDefinitions',N'Vehicle Make', N'ar', N'صناعة المركبات', N's'),(N'LookupDefinitions',N'Vehicle Make', N'am', N'የተሽከርካሪ ስራ', N's'),(N'LookupDefinitions',N'Vehicle Make', N'cn', N'车辆制造', N's'),(N'LookupDefinitions',N'Vehicle Makes', N'ar', N'يجعل السيارة', N'p'),(N'LookupDefinitions',N'Vehicle Makes', N'am', N'የተሽከርካሪ መኪናዎች ያደርጉታል', N'p'),(N'LookupDefinitions',N'Vehicle Makes', N'cn', N'车辆制造', N'p'),
(N'LookupDefinitions',N'Thickness', N'ar', N'سماكة', N's'),(N'LookupDefinitions',N'Thickness', N'am', N'ውፍረት', N's'),(N'LookupDefinitions',N'Thickness', N'cn', N'厚度', N's'),(N'LookupDefinitions',N'Thicknesses', N'ar', N'السماكات', N'p'),(N'LookupDefinitions',N'Thicknesses', N'am', N'ወፍራም', N'p'),(N'LookupDefinitions',N'Thicknesses', N'cn', N'厚度', N'p'),
(N'LookupDefinitions',N'Paper Origin', N'ar', N'أصل الورق', N's'),(N'LookupDefinitions',N'Paper Origin', N'am', N'የወረቀት አመጣጥ', N's'),(N'LookupDefinitions',N'Paper Origin', N'cn', N'纸张来源', N's'),(N'LookupDefinitions',N'Paper Origins', N'ar', N'أصول الورق', N'p'),(N'LookupDefinitions',N'Paper Origins', N'am', N'የወረቀት አመጣጥ', N'p'),(N'LookupDefinitions',N'Paper Origins', N'cn', N'纸张来源', N'p'),
(N'LookupDefinitions',N'Paper Group', N'ar', N'مجموعة الورق', N's'),(N'LookupDefinitions',N'Paper Group', N'am', N'የወረቀት ቡድን', N's'),(N'LookupDefinitions',N'Paper Group', N'cn', N'纸业集团', N's'),(N'LookupDefinitions',N'Paper Groups', N'ar', N'مجموعات الورق', N'p'),(N'LookupDefinitions',N'Paper Groups', N'am', N'የወረቀት ቡድኖች', N'p'),(N'LookupDefinitions',N'Paper Groups', N'cn', N'纸组', N'p'),
(N'LookupDefinitions',N'Paper Type', N'ar', N'نوع الورق', N's'),(N'LookupDefinitions',N'Paper Type', N'am', N'የወረቀት ዓይነት', N's'),(N'LookupDefinitions',N'Paper Type', N'cn', N'纸张类型', N's'),(N'LookupDefinitions',N'Paper Types', N'ar', N'أنواع الورق', N'p'),(N'LookupDefinitions',N'Paper Types', N'am', N'የወረቀት ዓይነቶች', N'p'),(N'LookupDefinitions',N'Paper Types', N'cn', N'纸张类型', N'p');
-- Resource Definitions
INSERT INTO dbo.Translations([TableName],[SourceEnglishWord],[DestinationCultureId],[DestinationWord],[Form]) VALUES
(N'ResourceDefinitions',N'Property, Plant, Equipment', N'ar', N'عقار - منشأة - آلة', N's'),(N'ResourceDefinitions',N'Property, Plant, Equipment', N'am', N'ሪል እስቴት - መገልገያ - ማሽን', N's'),(N'ResourceDefinitions',N'Property, Plant, Equipment', N'cn', N'房地产-设施-机器', N's'),(N'ResourceDefinitions',N'Property, plant, and equipment', N'ar', N'عقارات، منشآت وآلات', N'p'),(N'ResourceDefinitions',N'Property, plant, and equipment', N'am', N'ሪል እስቴት ፣ ተክል እና መሳሪያ', N'p'),(N'ResourceDefinitions',N'Property, plant, and equipment', N'cn', N'房地产，厂房和设备', N'p'),
(N'ResourceDefinitions',N'Office Equipment', N'ar', N'أداة مكتب', N's'),(N'ResourceDefinitions',N'Office Equipment', N'am', N'የቢሮ መሳሪያ', N's'),(N'ResourceDefinitions',N'Office Equipment', N'cn', N'办公工具', N's'),(N'ResourceDefinitions',N'Office Equipment', N'ar', N'معدات مكتبية', N'p'),(N'ResourceDefinitions',N'Office Equipment', N'am', N'የቢሮ መሣሪያዎች', N'p'),(N'ResourceDefinitions',N'Office Equipment', N'cn', N'办公用品', N'p'),
(N'ResourceDefinitions',N'Computer Equipment', N'ar', N'جهاز كمبيوتر', N's'),(N'ResourceDefinitions',N'Computer Equipment', N'am', N'ፒሲ', N's'),(N'ResourceDefinitions',N'Computer Equipment', N'cn', N'个人电脑', N's'),(N'ResourceDefinitions',N'Computer Equipment', N'ar', N'أجهزة كمبيوتر', N'p'),(N'ResourceDefinitions',N'Computer Equipment', N'am', N'ኮምፒተሮች', N'p'),(N'ResourceDefinitions',N'Computer Equipment', N'cn', N'电脑', N'p'),
(N'ResourceDefinitions',N'Machinery', N'ar', N'آلة', N's'),(N'ResourceDefinitions',N'Machinery', N'am', N'ማሽኖች', N's'),(N'ResourceDefinitions',N'Machinery', N'cn', N'机械', N's'),(N'ResourceDefinitions',N'Machineries', N'ar', N'آليات', N'p'),(N'ResourceDefinitions',N'Machineries', N'am', N'ማሽኖች', N'p'),(N'ResourceDefinitions',N'Machineries', N'cn', N'机械设备', N'p'),
(N'ResourceDefinitions',N'Vehicles', N'ar', N'مركبة', N's'),(N'ResourceDefinitions',N'Vehicles', N'am', N'ተሽከርካሪዎች', N's'),(N'ResourceDefinitions',N'Vehicles', N'cn', N'汽车', N's'),(N'ResourceDefinitions',N'Vehicles', N'ar', N'مركبات', N'p'),(N'ResourceDefinitions',N'Vehicles', N'am', N'ተሽከርካሪዎች', N'p'),(N'ResourceDefinitions',N'Vehicles', N'cn', N'汽车', N'p'),
(N'ResourceDefinitions',N'Building', N'ar', N'مبنى', N's'),(N'ResourceDefinitions',N'Building', N'am', N'ህንፃዎች', N's'),(N'ResourceDefinitions',N'Building', N'cn', N'建筑物', N's'),(N'ResourceDefinitions',N'Buildings', N'ar', N'مباني', N'p'),(N'ResourceDefinitions',N'Buildings', N'am', N'ህንፃዎች', N'p'),(N'ResourceDefinitions',N'Buildings', N'cn', N'建筑物', N'p'),
(N'ResourceDefinitions',N'Investment Property', N'ar', N'عقار استثماري', N's'),(N'ResourceDefinitions',N'Investment Property', N'am', N'የኢንmentስትሜንት ንብረቶች', N's'),(N'ResourceDefinitions',N'Investment Property', N'cn', N'投资物业', N's'),(N'ResourceDefinitions',N'Investment Properties', N'ar', N'عقارات استثمارية', N'p'),(N'ResourceDefinitions',N'Investment Properties', N'am', N'የኢንmentስትሜንት ንብረቶች', N'p'),(N'ResourceDefinitions',N'Investment Properties', N'cn', N'投资物业', N'p'),
(N'ResourceDefinitions',N'Raw Grain', N'ar', N'حب خام', N's'),(N'ResourceDefinitions',N'Raw Grain', N'am', N'ጥሬ እህሎች', N's'),(N'ResourceDefinitions',N'Raw Grain', N'cn', N'原始谷物', N's'),(N'ResourceDefinitions',N'Raw Grains', N'ar', N'حبوب خام', N'p'),(N'ResourceDefinitions',N'Raw Grains', N'am', N'ጥሬ እህሎች', N'p'),(N'ResourceDefinitions',N'Raw Grains', N'cn', N'原始谷物', N'p'),
(N'ResourceDefinitions',N'Cleaned Grain', N'ar', N'حب نظيف', N's'),(N'ResourceDefinitions',N'Cleaned Grain', N'am', N'የተጣራ እህል', N's'),(N'ResourceDefinitions',N'Cleaned Grain', N'cn', N'清洁谷物', N's'),(N'ResourceDefinitions',N'Cleaned Grains', N'ar', N'حبوب نظيفة', N'p'),(N'ResourceDefinitions',N'Cleaned Grains', N'am', N'የተጣራ እህል', N'p'),(N'ResourceDefinitions',N'Cleaned Grains', N'cn', N'清洁谷物', N'p'),
(N'ResourceDefinitions',N'Reject Grain', N'ar', N'مخلف حبوب', N's'),(N'ResourceDefinitions',N'Reject Grain', N'am', N'እህልን ይከልክሉ', N's'),(N'ResourceDefinitions',N'Reject Grain', N'cn', N'拒绝谷物', N's'),(N'ResourceDefinitions',N'Reject Grains', N'ar', N'مخلفات حبوب', N'p'),(N'ResourceDefinitions',N'Reject Grains', N'am', N'እህልን ይከልክሉ', N'p'),(N'ResourceDefinitions',N'Reject Grains', N'cn', N'拒绝谷物', N'p'),
(N'ResourceDefinitions',N'Vehicles Component', N'ar', N'مكون المركبات', N's'),(N'ResourceDefinitions',N'Vehicles Component', N'am', N'የተሽከርካሪዎች ክፍሎች', N's'),(N'ResourceDefinitions',N'Vehicles Component', N'cn', N'车辆零部件', N's'),(N'ResourceDefinitions',N'Vehicles Components', N'ar', N'مكونات مركبات', N'p'),(N'ResourceDefinitions',N'Vehicles Components', N'am', N'የተሽከርካሪዎች ክፍሎች', N'p'),(N'ResourceDefinitions',N'Vehicles Components', N'cn', N'车辆零部件', N'p'),
(N'ResourceDefinitions',N'Assembled Vehicle', N'ar', N'مركبة مجمعة', N's'),(N'ResourceDefinitions',N'Assembled Vehicle', N'am', N'የተሰበሰቡ ተሽከርካሪዎች', N's'),(N'ResourceDefinitions',N'Assembled Vehicle', N'cn', N'组装车', N's'),(N'ResourceDefinitions',N'Assembled Vehicles', N'ar', N'مركبات مجمعة', N'p'),(N'ResourceDefinitions',N'Assembled Vehicles', N'am', N'የተሰበሰቡ ተሽከርካሪዎች', N'p'),(N'ResourceDefinitions',N'Assembled Vehicles', N'cn', N'组装车', N'p'),
(N'ResourceDefinitions',N'Raw Material (Oil Milling)', N'ar', N'المواد الخام (طحن النفط)', N's'),(N'ResourceDefinitions',N'Raw Material (Oil Milling)', N'am', N'ጥሬ እቃዎች (ዘይት ቁፋሮ)', N's'),(N'ResourceDefinitions',N'Raw Material (Oil Milling)', N'cn', N'原料（制油）', N's'),(N'ResourceDefinitions',N'Raw Materials (Oil Milling)', N'ar', N'مواد خام (عصر زيوت)', N'p'),(N'ResourceDefinitions',N'Raw Materials (Oil Milling)', N'am', N'ጥሬ እቃዎች (ዘይት ቁፋሮ)', N'p'),(N'ResourceDefinitions',N'Raw Materials (Oil Milling)', N'cn', N'原料（制油）', N'p'),
(N'ResourceDefinitions',N'Processed Oil (Milling)', N'ar', N'زيت معالج (طحن)', N's'),(N'ResourceDefinitions',N'Processed Oil (Milling)', N'am', N'የተቀቀለ ዘይት (ወፍጮ)', N's'),(N'ResourceDefinitions',N'Processed Oil (Milling)', N'cn', N'成品油（铣削）', N's'),(N'ResourceDefinitions',N'Processed Oil (Milling)', N'ar', N'زيت معصور', N'p'),(N'ResourceDefinitions',N'Processed Oil (Milling)', N'am', N'የተቀቀለ ዘይት (ወፍጮ)', N'p'),(N'ResourceDefinitions',N'Processed Oil (Milling)', N'cn', N'成品油（铣削）', N'p'),
(N'ResourceDefinitions',N'Oil Byproduct', N'ar', N'منتج ثانوي (زيوت)', N's'),(N'ResourceDefinitions',N'Oil Byproduct', N'am', N'የዘይት ፍሬ', N's'),(N'ResourceDefinitions',N'Oil Byproduct', N'cn', N'石油副产品', N's'),(N'ResourceDefinitions',N'Oil Byproducts', N'ar', N'منتجات ثانوية (زيوت)', N'p'),(N'ResourceDefinitions',N'Oil Byproducts', N'am', N'ዘይት ያመርታል', N'p'),(N'ResourceDefinitions',N'Oil Byproducts', N'cn', N'石油副产品', N'p'),
(N'ResourceDefinitions',N'Work in Progress', N'ar', N'إنتاج قيد التنفيذ', N's'),(N'ResourceDefinitions',N'Work in Progress', N'am', N'ገና በሂደት ላይ ያለ ስራ', N's'),(N'ResourceDefinitions',N'Work in Progress', N'cn', N'工作正在进行中', N's'),(N'ResourceDefinitions',N'Work In Progress', N'ar', N'منتجات قيد التنفيذ', N'p'),(N'ResourceDefinitions',N'Work In Progress', N'am', N'ገና በሂደት ላይ ያለ ስራ', N'p'),(N'ResourceDefinitions',N'Work In Progress', N'cn', N'工作正在进行中', N'p'),
(N'ResourceDefinitions',N'Medicine', N'ar', N'دواء', N's'),(N'ResourceDefinitions',N'Medicine', N'am', N'መድሃኒት', N's'),(N'ResourceDefinitions',N'Medicine', N'cn', N'药物', N's'),(N'ResourceDefinitions',N'Medicines', N'ar', N'أدوية', N'p'),(N'ResourceDefinitions',N'Medicines', N'am', N'መድሃኒቶች', N'p'),(N'ResourceDefinitions',N'Medicines', N'cn', N'药物', N'p'),
(N'ResourceDefinitions',N'Construction Material', N'ar', N'مادة بناء', N's'),(N'ResourceDefinitions',N'Construction Material', N'am', N'የግንባታ ቁሳቁሶች', N's'),(N'ResourceDefinitions',N'Construction Material', N'cn', N'建筑材料', N's'),(N'ResourceDefinitions',N'Construction Materials', N'ar', N'مواد بناء', N'p'),(N'ResourceDefinitions',N'Construction Materials', N'am', N'የግንባታ ቁሳቁሶች', N'p'),(N'ResourceDefinitions',N'Construction Materials', N'cn', N'建筑材料', N'p'),
(N'ResourceDefinitions',N'Employee Benefit', N'ar', N'بند موظف', N's'),(N'ResourceDefinitions',N'Employee Benefit', N'am', N'የሰራተኛ ጥቅም', N's'),(N'ResourceDefinitions',N'Employee Benefit', N'cn', N'员工福利', N's'),(N'ResourceDefinitions',N'Employee Benefits', N'ar', N'بنود موظف', N'p'),(N'ResourceDefinitions',N'Employee Benefits', N'am', N'የሰራተኛ ጥቅሞች', N'p'),(N'ResourceDefinitions',N'Employee Benefits', N'cn', N'员工福利', N'p'),
(N'ResourceDefinitions',N'Revenue Service', N'ar', N'خدمة (إيراد)', N's'),(N'ResourceDefinitions',N'Revenue Service', N'am', N'የገቢ አገልግሎት', N's'),(N'ResourceDefinitions',N'Revenue Service', N'cn', N'税收服务', N's'),(N'ResourceDefinitions',N'Revenue Services', N'ar', N'خدمات (إيرادات)', N'p'),(N'ResourceDefinitions',N'Revenue Services', N'am', N'የገቢ አገልግሎቶች', N'p'),(N'ResourceDefinitions',N'Revenue Services', N'cn', N'税收服务', N'p');

-- Contract Definitions
INSERT INTO dbo.Translations([TableName],[SourceEnglishWord],[DestinationCultureId],[DestinationWord],[Form]) VALUES
(N'ContractDefinitions',N'Creditor', N'ar', N'الدائن', N's'),(N'ContractDefinitions',N'Creditor', N'am', N'አበዳሪ', N's'),(N'ContractDefinitions',N'Creditor', N'cn', N'债权人', N's'),(N'ContractDefinitions',N'Creditors', N'ar', N'الدائنين', N'p'),(N'ContractDefinitions',N'Creditors', N'am', N'አበዳሪዎች', N'p'),(N'ContractDefinitions',N'Creditors', N'cn', N'债权人', N'p'),
(N'ContractDefinitions',N'Debtor', N'ar', N'المدين', N's'),(N'ContractDefinitions',N'Debtor', N'am', N'አበዳሪ', N's'),(N'ContractDefinitions',N'Debtor', N'cn', N'债务人', N's'),(N'ContractDefinitions',N'Debtors', N'ar', N'المدينين', N'p'),(N'ContractDefinitions',N'Debtors', N'am', N'አበዳሪዎች', N'p'),(N'ContractDefinitions',N'Debtors', N'cn', N'债务人', N'p'),
(N'ContractDefinitions',N'Owner', N'ar', N'المالك', N's'),(N'ContractDefinitions',N'Owner', N'am', N'ባለቤት', N's'),(N'ContractDefinitions',N'Owner', N'cn', N'所有者', N's'),(N'ContractDefinitions',N'Owners', N'ar', N'المالكين', N'p'),(N'ContractDefinitions',N'Owners', N'am', N'ባለቤቶች', N'p'),(N'ContractDefinitions',N'Owners', N'cn', N'拥有者', N'p'),
(N'ContractDefinitions',N'Partner', N'ar', N'الشريك', N's'),(N'ContractDefinitions',N'Partner', N'am', N'አጋር', N's'),(N'ContractDefinitions',N'Partner', N'cn', N'伙伴', N's'),(N'ContractDefinitions',N'Partners', N'ar', N'الشركاء', N'p'),(N'ContractDefinitions',N'Partners', N'am', N'አጋሮች', N'p'),(N'ContractDefinitions',N'Partners', N'cn', N'伙伴', N'p'),
(N'ContractDefinitions',N'Supplier', N'ar', N'المورد', N's'),(N'ContractDefinitions',N'Supplier', N'am', N'አቅራቢ', N's'),(N'ContractDefinitions',N'Supplier', N'cn', N'供应商', N's'),(N'ContractDefinitions',N'Suppliers', N'ar', N'الموردين', N'p'),(N'ContractDefinitions',N'Suppliers', N'am', N'አቅራቢዎች', N'p'),(N'ContractDefinitions',N'Suppliers', N'cn', N'供应商', N'p'),
(N'ContractDefinitions',N'Customer', N'ar', N'الزبون', N's'),(N'ContractDefinitions',N'Customer', N'am', N'ደንበኛው', N's'),(N'ContractDefinitions',N'Customer', N'cn', N'顾客', N's'),(N'ContractDefinitions',N'Customers', N'ar', N'الزبائن', N'p'),(N'ContractDefinitions',N'Customers', N'am', N'ደንበኞች', N'p'),(N'ContractDefinitions',N'Customers', N'cn', N'顾客', N'p'),
(N'ContractDefinitions',N'Employee', N'ar', N'الموظف', N's'),(N'ContractDefinitions',N'Employee', N'am', N'ተቀጣሪ', N's'),(N'ContractDefinitions',N'Employee', N'cn', N'雇员', N's'),(N'ContractDefinitions',N'Employees', N'ar', N'الموظفين', N'p'),(N'ContractDefinitions',N'Employees', N'am', N'ሠራተኞች', N'p'),(N'ContractDefinitions',N'Employees', N'cn', N'雇员', N'p'),
(N'ContractDefinitions',N'Bank Account', N'ar', N'حساب البنك', N's'),(N'ContractDefinitions',N'Bank Account', N'am', N'የባንክ ሒሳብ', N's'),(N'ContractDefinitions',N'Bank Account', N'cn', N'银行账户', N's'),(N'ContractDefinitions',N'Bank Accounts', N'ar', N'الحسابات البنكية', N'p'),(N'ContractDefinitions',N'Bank Accounts', N'am', N'የባንክ ሂሳቦች', N'p'),(N'ContractDefinitions',N'Bank Accounts', N'cn', N'银行账户', N'p'),
(N'ContractDefinitions',N'Petty Cash Fund', N'ar', N'النثرية', N's'),(N'ContractDefinitions',N'Petty Cash Fund', N'am', N'የቤት እንስሳት ገንዘብ ፈንድ', N's'),(N'ContractDefinitions',N'Petty Cash Fund', N'cn', N'小额现金基金', N's'),(N'ContractDefinitions',N'Petty Cash Funds', N'ar', N'النثريات', N'p'),(N'ContractDefinitions',N'Petty Cash Funds', N'am', N'የቤት እንስሳት ጥሬ ገንዘብ', N'p'),(N'ContractDefinitions',N'Petty Cash Funds', N'cn', N'小额现金基金', N'p'),
(N'ContractDefinitions',N'Cashier', N'ar', N'الصراف', N's'),(N'ContractDefinitions',N'Cashier', N'am', N'ገንዘብ ተቀባይ', N's'),(N'ContractDefinitions',N'Cashier', N'cn', N'出纳员', N's'),(N'ContractDefinitions',N'Cashiers', N'ar', N'الصرافين', N'p'),(N'ContractDefinitions',N'Cashiers', N'am', N'ገንዘብ ተቀባይ', N'p'),(N'ContractDefinitions',N'Cashiers', N'cn', N'收银员', N'p'),
(N'ContractDefinitions',N'Warehouse', N'ar', N'المستودع', N's'),(N'ContractDefinitions',N'Warehouse', N'am', N'መጋዘን', N's'),(N'ContractDefinitions',N'Warehouse', N'cn', N'仓库', N's'),(N'ContractDefinitions',N'Warehouses', N'ar', N'المستودعات', N'p'),(N'ContractDefinitions',N'Warehouses', N'am', N'መጋዘኖች', N'p'),(N'ContractDefinitions',N'Warehouses', N'cn', N'货仓', N'p'),
(N'ContractDefinitions',N'Foreign Import', N'ar', N'الاستيراد الخارجي', N's'),(N'ContractDefinitions',N'Foreign Import', N'am', N'የውጭ አስመጪ', N's'),(N'ContractDefinitions',N'Foreign Import', N'cn', N'国外进口', N's'),(N'ContractDefinitions',N'Foreign Imports', N'ar', N'الواردات الأجنبية', N'p'),(N'ContractDefinitions',N'Foreign Imports', N'am', N'የውጭ ማስመጣት', N'p'),(N'ContractDefinitions',N'Foreign Imports', N'cn', N'国外进口', N'p'),
(N'ContractDefinitions',N'Foreign Export', N'ar', N'التصدير الخارجي', N's'),(N'ContractDefinitions',N'Foreign Export', N'am', N'የውጭ መላኪያ', N's'),(N'ContractDefinitions',N'Foreign Export', N'cn', N'对外出口', N's'),(N'ContractDefinitions',N'Foreign Exports', N'ar', N'الصادرات الأجنبية', N'p'),(N'ContractDefinitions',N'Foreign Exports', N'am', N'የውጭ ንግድ', N'p'),(N'ContractDefinitions',N'Foreign Exports', N'cn', N'国外出口', N'p');

-- Users
UPDATE dbo.Users
SET
	[Name]  = dbo.fn_TranslateFromEnglish(N'Users', [Name], @PrimaryLanguageId, 'n'),
	[Name2] = dbo.fn_TranslateFromEnglish(N'Users', [Name], @SecondaryLanguageId, 'n'),
	[Name3] = dbo.fn_TranslateFromEnglish(N'Users', [Name], @TernaryLanguageId, 'n')
WHERE [Name] IN (SELECT [SourceEnglishWord] FROM dbo.Translations WHERE [TableName] = N'Users' AND [Form] = N'n')
-- Roles
UPDATE dbo.Roles
SET
	[Name]  = dbo.fn_TranslateFromEnglish(N'Roles', [Name], @PrimaryLanguageId, 'n'),
	[Name2] = dbo.fn_TranslateFromEnglish(N'Roles', [Name], @SecondaryLanguageId, 'n'),
	[Name3] = dbo.fn_TranslateFromEnglish(N'Roles', [Name], @TernaryLanguageId, 'n')
WHERE [Name] IN (SELECT [SourceEnglishWord] FROM dbo.Translations WHERE [TableName] = N'Roles' AND [Form] = N'n')
-- Entry Types
-- Account Types
-- IfrsConcepts
-- IfrsDisclosures
-- Currencies
-- Report Definitions
-- Lookup Definitions
UPDATE dbo.LookupDefinitions
SET
	[TitleSingular]  = dbo.fn_TranslateFromEnglish(N'LookupDefinitions', [TitleSingular], @PrimaryLanguageId, 's'),
	[TitleSingular2] = dbo.fn_TranslateFromEnglish(N'LookupDefinitions', [TitleSingular], @SecondaryLanguageId, 's'),
	[TitleSingular3] = dbo.fn_TranslateFromEnglish(N'LookupDefinitions', [TitleSingular], @TernaryLanguageId, 's'),
	[TitlePlural]  = dbo.fn_TranslateFromEnglish(N'LookupDefinitions', [TitlePlural], @PrimaryLanguageId, 'p'),
	[TitlePlural2] = dbo.fn_TranslateFromEnglish(N'LookupDefinitions', [TitlePlural], @SecondaryLanguageId, 'p'),
	[TitlePlural3] = dbo.fn_TranslateFromEnglish(N'LookupDefinitions', [TitlePlural], @TernaryLanguageId, 'p')
WHERE [TitleSingular] IN (SELECT [SourceEnglishWord] FROM dbo.Translations WHERE [TableName] = N'LookupDefinitions' AND [Form] = N's')
AND [TitlePlural] IN (SELECT [SourceEnglishWord] FROM dbo.Translations WHERE [TableName] = N'LookupDefinitions' AND [Form] = N'p')
-- Resource Definitions
UPDATE dbo.ResourceDefinitions
SET
	[TitleSingular]  = dbo.fn_TranslateFromEnglish(N'ResourceDefinitions', [TitleSingular], @PrimaryLanguageId, 's'),
	[TitleSingular2] = dbo.fn_TranslateFromEnglish(N'ResourceDefinitions', [TitleSingular], @SecondaryLanguageId, 's'),
	[TitleSingular3] = dbo.fn_TranslateFromEnglish(N'ResourceDefinitions', [TitleSingular], @TernaryLanguageId, 's'),
	[TitlePlural]  = dbo.fn_TranslateFromEnglish(N'ResourceDefinitions', [TitlePlural], @PrimaryLanguageId, 'p'),
	[TitlePlural2] = dbo.fn_TranslateFromEnglish(N'ResourceDefinitions', [TitlePlural], @SecondaryLanguageId, 'p'),
	[TitlePlural3] = dbo.fn_TranslateFromEnglish(N'ResourceDefinitions', [TitlePlural], @TernaryLanguageId, 'p')
WHERE [TitleSingular] IN (SELECT [SourceEnglishWord] FROM dbo.Translations WHERE [TableName] = N'ResourceDefinitions' AND [Form] = N's')
AND [TitlePlural] IN (SELECT [SourceEnglishWord] FROM dbo.Translations WHERE [TableName] = N'ResourceDefinitions' AND [Form] = N'p');
-- Contract Definitions
UPDATE dbo.ContractDefinitions
SET
	[TitleSingular]  = dbo.fn_TranslateFromEnglish(N'ContractDefinitions', [TitleSingular], @PrimaryLanguageId, 's'),
	[TitleSingular2] = dbo.fn_TranslateFromEnglish(N'ContractDefinitions', [TitleSingular], @SecondaryLanguageId, 's'),
	[TitleSingular3] = dbo.fn_TranslateFromEnglish(N'ContractDefinitions', [TitleSingular], @TernaryLanguageId, 's'),
	[TitlePlural]  = dbo.fn_TranslateFromEnglish(N'ContractDefinitions', [TitlePlural], @PrimaryLanguageId, 'p'),
	[TitlePlural2] = dbo.fn_TranslateFromEnglish(N'ContractDefinitions', [TitlePlural], @SecondaryLanguageId, 'p'),
	[TitlePlural3] = dbo.fn_TranslateFromEnglish(N'ContractDefinitions', [TitlePlural], @TernaryLanguageId, 'p')
WHERE [TitleSingular] IN (SELECT [SourceEnglishWord] FROM dbo.Translations WHERE [TableName] = N'ContractDefinitions' AND [Form] = N's')
AND [TitlePlural] IN (SELECT [SourceEnglishWord] FROM dbo.Translations WHERE [TableName] = N'ContractDefinitions' AND [Form] = N'p');

-- Line Definitions
-- Document Definitions