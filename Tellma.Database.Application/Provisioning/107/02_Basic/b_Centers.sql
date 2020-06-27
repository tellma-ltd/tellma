INSERT INTO @Centers([Index],[ParentIndex], [Name],[Name2],[Code], [CenterType]) VALUES
(-1,NULL, N'SSIA', N'الجهاز الاستثماري', N'', N'Segment'),
(0,-1, N'SSIA', N'الجهاز الاستثماري', N'0000', N'Parent'),
(1000,-1, N'Consolidated Board of Directors', N'مجلس الإدارة الموحد', N'1000', N'SellingGeneralAndAdministration'),
(2000,-1, N'Board of Commissioners', N'مجلس المفوضين ', N'2000', N'SellingGeneralAndAdministration'),
(3,-1, N'Executive Management', N'الإدارة التنفيذية', N'3', N'Abstract'),
(31,3, N'financial affairs', N'الشئون المالية ', N'31', N'Abstract'),
(3100,31, N'Directorate General for Financial Affairs', N'الإدارة العامة للشئون المالية ', N'3100', N'SellingGeneralAndAdministration'),
(311,31, N'Finance', N'المالية ', N'311', N'Abstract'),
(3110,311, N'Finance Department', N'الإدارة المالية ', N'3110', N'SellingGeneralAndAdministration'),
(3111,311, N'Accounts Division', N'قسم الحسابات', N'3111', N'SellingGeneralAndAdministration'),
(3112,311, N'Final Accounts Division', N'قسم الحسابات الختامية', N'3112', N'SellingGeneralAndAdministration'),
(3113,311, N'Collection Division', N'قسم التحصيل', N'3113', N'SellingGeneralAndAdministration'),
(3114,311, N'Payments Division', N'قسم الدفعيات', N'3114', N'SellingGeneralAndAdministration'),
(3115,311, N'Procurement Division', N'قسم المشتريات', N'3115', N'SellingGeneralAndAdministration'),
(32,3, N'Human Resources and Administrative Affairs', N'تنمية الموارد البشرية والشئون الادارية ', N'32', N'Abstract'),
(3200,32, N'Directorate General for Human Resources and Administrative Affairs', N'الإدارة العامة لتنمية الموارد البشرية والشئون الادارية ', N'3200', N'SellingGeneralAndAdministration'),
(321,32, N'Human Resource Development', N'تنمية الموارد البشرية ', N'321', N'Abstract'),
(3210,321, N'Human Resources Development Department', N'إدارة تنمية الموارد البشرية', N'3210', N'SellingGeneralAndAdministration'),
(3211,321, N'Personnel Division', N'قسم شئون العاملين', N'3211', N'SellingGeneralAndAdministration'),
(3212,321, N'Training Division', N'قسم التدريب', N'3212', N'SellingGeneralAndAdministration'),
(322,32, N'Administrative Affairs', N'الشئون الادارية ', N'322', N'Abstract'),
(3220,322, N'Administrative Affairs Department', N'إدارة الشئون الادارية', N'3220', N'SellingGeneralAndAdministration'),
(3221,322, N'Services Division', N'قسم الخدمات', N'3221', N'SellingGeneralAndAdministration'),
(3222,322, N'Security and Safety Division', N'قسم الأمن والسلامة', N'3222', N'SellingGeneralAndAdministration'),
(33,3, N'Corporate and Real Estate Development', N'الشركات والتنمية العقارية ', N'33', N'Abstract'),
(3300,33, N'Public administration companies and Real Estate Development', N'الإدارة العامة للشركات والتنمية العقارية', N'3300', N'SellingGeneralAndAdministration'),
(3310,33, N'Department of Agricultural Investment', N'إدارة الاستثمار الزراعي', N'3310', N'SellingGeneralAndAdministration'),
(3320,33, N'Department of Tourism Investment and Hotels', N'إدارة الاستثمار السياحي والفنادق', N'3320', N'SellingGeneralAndAdministration'),
(3330,33, N'Department of Industrial Investment', N'إدارة الاستثمار الصناعي', N'3330', N'SellingGeneralAndAdministration'),
(3340,33, N'Department of Medical Investment', N'إدارة الاستثمار الطبي', N'3340', N'SellingGeneralAndAdministration'),
(335,33, N'Marketing', N'التسويق', N'335', N'Abstract'),
(3350,335, N'Marketing Department', N'إدارة التسويق', N'3350', N'SellingGeneralAndAdministration'),
(3351,335, N'Commercial Marketing', N'التسويق التجاري', N'3351', N'SellingGeneralAndAdministration'),
(3352,335, N'Real Estate Marketing', N'التسويق العقاري', N'3352', N'SellingGeneralAndAdministration'),
(336,33, N'Real Estate Development', N'التنمية العقارية', N'336', N'Abstract'),
(3360,336, N'Real Estate Development Department', N'إدارة التنمية العقارية', N'3360', N'SellingGeneralAndAdministration'),
(3361,336, N'Development Division', N'قسم التطوير العقاري', N'3361', N'SellingGeneralAndAdministration'),
(3362,336, N'Projects Division', N'قسم المشروعات', N'3362', N'SellingGeneralAndAdministration'),
(34,3, N'Studies and Strategic Planning', N'الدراسات والتخطيط الاستراتيجي ', N'34', N'Abstract'),
(3400,34, N'Directorate General for Studies and Strategic Planning', N'الإدارة العامة للدراسات والتخطيط الاستراتيجي', N'3400', N'SellingGeneralAndAdministration'),
(341,34, N'Studies and Research', N'الدراسات والبحوث', N'341', N'Abstract'),
(3410,341, N'Studies and Research Department', N'إدارة الدراسات والبحوث', N'3410', N'SellingGeneralAndAdministration'),
(3411,341, N'Risk Department', N'قسم المخاطر', N'3411', N'SellingGeneralAndAdministration'),
(3412,341, N'Total Quality Department', N'قسم الجودة الشاملة', N'3412', N'SellingGeneralAndAdministration'),
(3413,341, N'Department of Feasibility Studies and Statistics', N'قسم دراسات الجدوى والاحصاء', N'3413', N'SellingGeneralAndAdministration'),
(3414,341, N'Research and Development Department', N'قسم البحوث والتطوير', N'3414', N'SellingGeneralAndAdministration'),
(342,34, N'Information Technology', N'تكنولوجيا المعلومات ', N'342', N'Abstract'),
(3420,342, N'Information Technology Management', N'إدارة تكنولوجيا المعلومات', N'3420', N'SellingGeneralAndAdministration'),
(3421,342, N'Technical Support and Maintenance Department', N'قسم الدعم الفني والصيانة', N'3421', N'SellingGeneralAndAdministration'),
(3422,342, N'Department of Network and Information Security', N'قسم الشبكات وامن المعلومات', N'3422', N'SellingGeneralAndAdministration'),
(3423,342, N'Systems Development and Software Division', N'قسم تطوير الانظمة والبرمجيات', N'3423', N'SellingGeneralAndAdministration'),
(35,3, N'Financial investment and smart partnerships', N'الاستثمار المالي والشراكات الذكية ', N'35', N'Abstract'),
(3500,35, N'Directorate General for Financial Investment and smart partnerships', N'الإدارة العامة للاستثمار المالي والشراكات الذكية ', N'3500', N'SellingGeneralAndAdministration'),
(3510,35, N'Banking System Management', N'إدارة الجهاز المصرفي', N'3510', N'SellingGeneralAndAdministration'),
(3520,35, N'Financial Services Department', N'إدارة الخدمات المالية', N'3520', N'SellingGeneralAndAdministration'),
(3530,35, N'Internal Trade Finance Management', N'إدارة التمويل التجاري الداخلي', N'3530', N'SellingGeneralAndAdministration'),
(39,3, N'Commissioner-General', N'المفوض العام', N'39', N'Abstract'),
(3900,39, N'Office of the Commissioner-General', N'مكتب المفوض العام', N'3900', N'SellingGeneralAndAdministration'),
(391,39, N'Public relations', N'العلاقات العامة', N'391', N'Abstract'),
(3910,391, N'Public Relations Department', N'إدارة العلاقات العامة', N'3910', N'SellingGeneralAndAdministration'),
(3911,391, N'Department of Media', N'قسم الإعلام', N'3911', N'SellingGeneralAndAdministration'),
(3912,391, N'Protocol Department', N'قسم المراسم', N'3912', N'SellingGeneralAndAdministration'),
(3920,39, N'Management Office Executive', N'إدارة المكتب التنفيذي', N'3920', N'SellingGeneralAndAdministration'),
(3930,39, N'Internal Audit Management', N'إدارة المراجعة الداخلية', N'3930', N'SellingGeneralAndAdministration'),
(3940,39, N'Legal Counsel', N'المستشار القانوني', N'3940', N'SellingGeneralAndAdministration'),
(4,-1, N'Real Estate', N'عقارات', N'4', N'Abstract'),
(41,4, N'Real Estate - Rentals', N'عقارات للتأجير', N'41', N'Abstract'),
(411,41, N'Saffat Towers Complex', N'مجمع أبراج الصافات', N'411', N'Abstract'),
(4111,411, N'Saffat Towers - Direct Expenses', N'أبراج الصافات - مصروفات مباشرة', N'4111', N'CostOfSales'),
(4112,411, N'Saffat Towers - SGA', N'أبراج الصافات - مصروفات غير مباشرة', N'4112', N'SellingGeneralAndAdministration'),
(412,41, N'Pearl Complex', N'مجمع لؤلؤة الثغر', N'412', N'Abstract'),
(4121,412, N'Pearl Complex - Direct Expenses', N'لؤلؤة الثغر - مصروفات مباشرة', N'4121', N'CostOfSales'),
(4122,412, N'Pearl Complex - SGA', N'لؤلؤة الثغر - مصروفات غير مباشرة', N'4122', N'SellingGeneralAndAdministration'),
(42,4, N'Real Estate - Sales', N'عقارات للبيع', N'42', N'Abstract'),
(421,42, N'Rayyan Scheme', N'مخطط الريان', N'421', N'Abstract'),
(4211,421, N'Rayyan Scheme - Cost of Sales', N'مخطط الريان - تكلفة المبيعات', N'4211', N'CostOfSales'),
(4212,421, N'Rayyan Scheme - SGA', N'مخطط الريان - مصروفات إدارية وتسويقية', N'4212', N'SellingGeneralAndAdministration'),
(422,42, N'Yasmine Scheme', N'مخطط الياسمين', N'422', N'Abstract'),
(4221,422, N'Yasmine Scheme - Cost of Sales', N'مخطط الياسمين - تكلفة المبيعات', N'4221', N'CostOfSales'),
(4222,422, N'Yasmine Scheme - SGA', N'مخطط الياسمين - مصروفات إدارية وتسويقية', N'4222', N'SellingGeneralAndAdministration'),
(5,422, N'Projects Under Construction', N'مشاريع قيد التنفيذ', N'5', N'Abstract'),
(5001,5, N'Ubayyid Hospital Project', N'مشروع مستشفى الأبيض', N'5001', N'ConstructionExpenseControl'),
(5002,5, N'Salam Scheme Project', N'مشروع مخطط السلام', N'5002', N'ConstructionExpenseControl'),
(5003,5, N'Ahfaad Complex Project', N'مشروع مجمع الأحفاد', N'5003', N'ConstructionExpenseControl');

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Declarations
-- Declarations
DECLARE @107C_SSIA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'SSIA');
DECLARE @107C_ConsolidatedBoardofDirectors INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Consolidated Board of Directors');
DECLARE @107C_BoardofCommissioners INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Board of Commissioners');
DECLARE @107C_DirectorateGeneralforFinancialAffairs INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Directorate General for Financial Affairs');
DECLARE @107C_FinanceDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Finance Department');
DECLARE @107C_AccountsDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Accounts Division');
DECLARE @107C_FinalAccountsDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Final Accounts Division');
DECLARE @107C_CollectionDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Collection Division');
DECLARE @107C_PaymentsDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Payments Division');
DECLARE @107C_ProcurementDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Procurement Division');
DECLARE @107C_DirectorateGeneralforHumanResourcesandAdministrativeAffairs INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Directorate General for Human Resources and Administrative Affairs');
DECLARE @107C_HumanResourcesDevelopmentDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Human Resources Development Department');
DECLARE @107C_PersonnelDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Personnel Division');
DECLARE @107C_TrainingDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Training Division');
DECLARE @107C_AdministrativeAffairsDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Administrative Affairs Department');
DECLARE @107C_ServicesDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Services Division');
DECLARE @107C_SecurityandSafetyDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Security and Safety Division');
DECLARE @107C_PublicadministrationcompaniesandRealEstateDevelopment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Public administration companies and Real Estate Development');
DECLARE @107C_DepartmentofAgriculturalInvestment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Department of Agricultural Investment');
DECLARE @107C_DepartmentofTourismInvestmentandHotels INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Department of Tourism Investment and Hotels');
DECLARE @107C_DepartmentofIndustrialInvestment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Department of Industrial Investment');
DECLARE @107C_DepartmentofMedicalInvestment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Department of Medical Investment');
DECLARE @107C_MarketingDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Marketing Department');
DECLARE @107C_CommercialMarketing INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Commercial Marketing');
DECLARE @107C_RealEstateMarketing INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Real Estate Marketing');
DECLARE @107C_RealEstateDevelopmentDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Real Estate Development Department');
DECLARE @107C_DevelopmentDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Development Division');
DECLARE @107C_ProjectsDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Projects Division');
DECLARE @107C_DirectorateGeneralforStudiesandStrategicPlanning INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Directorate General for Studies and Strategic Planning');
DECLARE @107C_StudiesandResearchDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Studies and Research Department');
DECLARE @107C_RiskDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Risk Department');
DECLARE @107C_TotalQualityDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Total Quality Department');
DECLARE @107C_DepartmentofFeasibilityStudiesandStatistics INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Department of Feasibility Studies and Statistics');
DECLARE @107C_ResearchandDevelopmentDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Research and Development Department');
DECLARE @107C_InformationTechnologyManagement INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Information Technology Management');
DECLARE @107C_TechnicalSupportandMaintenanceDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Technical Support and Maintenance Department');
DECLARE @107C_DepartmentofNetworkandInformationSecurity INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Department of Network and Information Security');
DECLARE @107C_SystemsDevelopmentandSoftwareDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Systems Development and Software Division');
DECLARE @107C_DirectorateGeneralforFinancialInvestmentandsmartpartnerships INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Directorate General for Financial Investment and smart partnerships');
DECLARE @107C_BankingSystemManagement INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Banking System Management');
DECLARE @107C_FinancialServicesDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Financial Services Department');
DECLARE @107C_InternalTradeFinanceManagement INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Internal Trade Finance Management');
DECLARE @107C_OfficeoftheCommissionerGeneral INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Office of the Commissioner-General');
DECLARE @107C_PublicRelationsDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Public Relations Department');
DECLARE @107C_DepartmentofMedia INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Department of Media');
DECLARE @107C_ProtocolDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Protocol Department');
DECLARE @107C_ManagementOfficeExecutive INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Management Office Executive');
DECLARE @107C_InternalAuditManagement INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Internal Audit Management');
DECLARE @107C_LegalCounsel INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Legal Counsel');
DECLARE @107C_SaffatTowersDirectExpenses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Saffat Towers - Direct Expenses');
DECLARE @107C_SaffatTowersSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Saffat Towers - SGA');
DECLARE @107C_PearlComplexDirectExpenses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Pearl Complex - Direct Expenses');
DECLARE @107C_PearlComplexSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Pearl Complex - SGA');
DECLARE @107C_RayyanSchemeCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Rayyan Scheme - Cost of Sales');
DECLARE @107C_RayyanSchemeSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Rayyan Scheme - SGA');
DECLARE @107C_YasmineSchemeCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Yasmine Scheme - Cost of Sales');
DECLARE @107C_YasmineSchemeSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Yasmine Scheme - SGA');
DECLARE @107C_UbayyidHospitalProject INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Ubayyid Hospital Project');
DECLARE @107C_SalamSchemeProject INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Salam Scheme Project');
DECLARE @107C_AhfaadComplexProject INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Ahfaad Complex Project');