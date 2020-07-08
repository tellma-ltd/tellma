DELETE FROM @Centers;
INSERT INTO @Centers([Index],[ParentIndex], [Name],[Name2],[Code], [CenterType]) VALUES
(0,NULL, N'SSIA', N'الجهاز الاستثماري', N'0', N'Abstract'),
(1,0, N'Headquarters', N'المقر الرئيسي', N'1', N'BusinessUnit'),
(1000,1, N'Boards', N'المجالس الإدارية', N'1000', N'Abstract'),
(10001,1000, N'Consolidated Board of Directors', N'مجلس الإدارة الموحد', N'10001', N'SellingGeneralAndAdministration'),
(10002,1000, N'Board of Commissioners', N'مجلس المفوضين ', N'10002', N'SellingGeneralAndAdministration'),
(12,1, N'Executive Management', N'الإدارة التنفيذية', N'12', N'Abstract'),
(121,12, N'financial affairs', N'الشئون المالية ', N'121', N'Abstract'),
(12100,121, N'Directorate General for Financial Affairs', N'الإدارة العامة للشئون المالية ', N'12100', N'SellingGeneralAndAdministration'),
(1211,121, N'Finance', N'المالية ', N'1211', N'Abstract'),
(12110,1211, N'Finance Department', N'الإدارة المالية ', N'12110', N'SellingGeneralAndAdministration'),
(12111,1211, N'Accounts Division', N'قسم الحسابات', N'12111', N'SellingGeneralAndAdministration'),
(12112,1211, N'Final Accounts Division', N'قسم الحسابات الختامية', N'12112', N'SellingGeneralAndAdministration'),
(12113,1211, N'Collection Division', N'قسم التحصيل', N'12113', N'SellingGeneralAndAdministration'),
(12114,1211, N'Payments Division', N'قسم الدفعيات', N'12114', N'SellingGeneralAndAdministration'),
(12115,1211, N'Procurement Division', N'قسم المشتريات', N'12115', N'SellingGeneralAndAdministration'),
(122,12, N'Human Resources and Administrative Affairs', N'تنمية الموارد البشرية والشئون الادارية ', N'122', N'Abstract'),
(12200,122, N'Directorate General for Human Resources and Administrative Affairs', N'الإدارة العامة لتنمية الموارد البشرية والشئون الادارية ', N'12200', N'SellingGeneralAndAdministration'),
(1221,122, N'Human Resource Development', N'تنمية الموارد البشرية ', N'1221', N'Abstract'),
(12210,1221, N'Human Resources Development Department', N'إدارة تنمية الموارد البشرية', N'12210', N'SellingGeneralAndAdministration'),
(12211,1221, N'Personnel Division', N'قسم شئون العاملين', N'12211', N'SellingGeneralAndAdministration'),
(12212,1221, N'Training Division', N'قسم التدريب', N'12212', N'SellingGeneralAndAdministration'),
(1222,122, N'Administrative Affairs', N'الشئون الادارية ', N'1222', N'Abstract'),
(12220,1222, N'Administrative Affairs Department', N'إدارة الشئون الادارية', N'12220', N'SellingGeneralAndAdministration'),
(12221,1222, N'Services Division', N'قسم الخدمات', N'12221', N'SellingGeneralAndAdministration'),
(12222,1222, N'Security and Safety Division', N'قسم الأمن والسلامة', N'12222', N'SellingGeneralAndAdministration'),
(123,12, N'Corporate and Real Estate Development', N'الشركات والتنمية العقارية ', N'123', N'Abstract'),
(12300,123, N'Public administration companies and Real Estate Development', N'الإدارة العامة للشركات والتنمية العقارية', N'12300', N'SellingGeneralAndAdministration'),
(1231,123, N'Agricultural Investment', N'الاستثمار الزراعي', N'1231', N'Abstract'),
(12310,1231, N'Department of Agricultural Investment', N'إدارة الاستثمار الزراعي', N'12310', N'SellingGeneralAndAdministration'),
(1232,123, N'Tourism Investment and Hotels', N'الاستثمار السياحي والفنادق', N'1232', N'Abstract'),
(12320,1232, N'Department of Tourism Investment and Hotels', N'إدارة الاستثمار السياحي والفنادق', N'12320', N'SellingGeneralAndAdministration'),
(1233,123, N'Industrial Investment', N'الاستثمار الصناعي', N'1233', N'Abstract'),
(12330,1233, N'Department of Industrial Investment', N'إدارة الاستثمار الصناعي', N'12330', N'SellingGeneralAndAdministration'),
(1234,123, N'Medical Investment', N'الاستثمار الطبي', N'1234', N'Abstract'),
(12340,1234, N'Department of Medical Investment', N'إدارة الاستثمار الطبي', N'12340', N'SellingGeneralAndAdministration'),
(1235,123, N'Marketing', N'التسويق', N'1235', N'Abstract'),
(12350,1235, N'Marketing Department', N'إدارة التسويق', N'12350', N'SellingGeneralAndAdministration'),
(12351,1235, N'Commercial Marketing', N'التسويق التجاري', N'12351', N'SellingGeneralAndAdministration'),
(12352,1235, N'Real Estate Marketing', N'التسويق العقاري', N'12352', N'SellingGeneralAndAdministration'),
(1236,123, N'Real Estate Development', N'التنمية العقارية', N'1236', N'Abstract'),
(12360,1236, N'Real Estate Development Department', N'إدارة التنمية العقارية', N'12360', N'SellingGeneralAndAdministration'),
(12361,1236, N'Development Division', N'قسم التطوير العقاري', N'12361', N'SellingGeneralAndAdministration'),
(12362,1236, N'Projects Division', N'قسم المشروعات', N'12362', N'SellingGeneralAndAdministration'),
(124,12, N'Studies and Strategic Planning', N'الدراسات والتخطيط الاستراتيجي ', N'124', N'Abstract'),
(12400,124, N'Directorate General for Studies and Strategic Planning', N'الإدارة العامة للدراسات والتخطيط الاستراتيجي', N'12400', N'SellingGeneralAndAdministration'),
(1241,124, N'Studies and Research', N'الدراسات والبحوث', N'1241', N'Abstract'),
(12410,1241, N'Studies and Research Department', N'إدارة الدراسات والبحوث', N'12410', N'SellingGeneralAndAdministration'),
(12411,1241, N'Risk Department', N'قسم المخاطر', N'12411', N'SellingGeneralAndAdministration'),
(12412,1241, N'Total Quality Department', N'قسم الجودة الشاملة', N'12412', N'SellingGeneralAndAdministration'),
(12413,1241, N'Department of Feasibility Studies and Statistics', N'قسم دراسات الجدوى والاحصاء', N'12413', N'SellingGeneralAndAdministration'),
(12414,1241, N'Research and Development Department', N'قسم البحوث والتطوير', N'12414', N'SellingGeneralAndAdministration'),
(1242,124, N'Information Technology', N'تكنولوجيا المعلومات ', N'1242', N'Abstract'),
(12420,1242, N'Information Technology Management', N'إدارة تكنولوجيا المعلومات', N'12420', N'SellingGeneralAndAdministration'),
(12421,1242, N'Technical Support and Maintenance Department', N'قسم الدعم الفني والصيانة', N'12421', N'SellingGeneralAndAdministration'),
(12422,1242, N'Department of Network and Information Security', N'قسم الشبكات وامن المعلومات', N'12422', N'SellingGeneralAndAdministration'),
(12423,1242, N'Systems Development and Software Division', N'قسم تطوير الانظمة والبرمجيات', N'12423', N'SellingGeneralAndAdministration'),
(125,12, N'Financial investment and smart partnerships', N'الاستثمار المالي والشراكات الذكية ', N'125', N'Abstract'),
(12500,125, N'Directorate General for Financial Investment and smart partnerships', N'الإدارة العامة للاستثمار المالي والشراكات الذكية ', N'12500', N'SellingGeneralAndAdministration'),
(1251,125, N'Banking System', N'الجهاز المصرفي', N'1251', N'Abstract'),
(12510,1251, N'Banking System Department', N'إدارة الجهاز المصرفي', N'12510', N'SellingGeneralAndAdministration'),
(1252,125, N'Financial Services', N'الخدمات المالية', N'1252', N'Abstract'),
(12520,1252, N'Financial Services Department', N'إدارة الخدمات المالية', N'12520', N'SellingGeneralAndAdministration'),
(1253,125, N'Internal Trade Finance', N'التمويل التجاري الداخلي', N'1253', N'Abstract'),
(12530,1253, N'Internal Trade Finance Department', N'إدارة التمويل التجاري الداخلي', N'12530', N'SellingGeneralAndAdministration'),
(129,12, N'Commissioner-General', N'المفوض العام', N'129', N'Abstract'),
(12900,129, N'Office of the Commissioner-General', N'مكتب المفوض العام', N'12900', N'SellingGeneralAndAdministration'),
(1291,129, N'Public relations', N'العلاقات العامة', N'1291', N'Abstract'),
(12910,1291, N'Public Relations Department', N'إدارة العلاقات العامة', N'12910', N'SellingGeneralAndAdministration'),
(12911,1291, N'Department of Media', N'قسم الإعلام', N'12911', N'SellingGeneralAndAdministration'),
(12912,1291, N'Protocol Department', N'قسم المراسم', N'12912', N'SellingGeneralAndAdministration'),
(12913,1291, N'Management Office Executive', N'إدارة المكتب التنفيذي', N'12913', N'SellingGeneralAndAdministration'),
(12914,1291, N'Internal Audit Management', N'إدارة المراجعة الداخلية', N'12914', N'SellingGeneralAndAdministration'),
(12915,1291, N'Legal Counsel', N'المستشار القانوني', N'12915', N'SellingGeneralAndAdministration'),
(2,0, N'Investment Property', N'الاستثمار العقاري', N'2', N'Abstract'),
(21,2, N'Saffat Towers Complex', N'مجمع أبراج الصافات', N'21', N'BusinessUnit'),
(21000,21, N'Saffat Towers - Direct Expenses', N'أبراج الصافات - مصروفات مباشرة', N'21000', N'CostOfSales'),
(211,21, N'Saffat Towers - SGA', N'أبراج الصافات - مصروفات غير مباشرة', N'211', N'Abstract'),
(21100,211, N'Saffat Towers - SGA', N'أبراج الصافات - مصروفات غير مباشرة', N'21100', N'SellingGeneralAndAdministration'),
(22,2, N'Pearl Complex', N'مجمع لؤلؤة الثغر', N'22', N'BusinessUnit'),
(22000,22, N'Pearl Complex - Direct Expenses', N'لؤلؤة الثغر - مصروفات مباشرة', N'22000', N'CostOfSales'),
(221,22, N'Pearl Complex - SGA', N'لؤلؤة الثغر - مصروفات غير مباشرة', N'221', N'Abstract'),
(22100,221, N'Pearl Complex - SGA', N'لؤلؤة الثغر - مصروفات غير مباشرة', N'22100', N'SellingGeneralAndAdministration'),
(23,2, N'Rayyan Scheme', N'مخطط الريان', N'23', N'BusinessUnit'),
(23000,23, N'Rayyan Scheme - Cost of Sales', N'مخطط الريان - مصروفات مباشرة', N'23000', N'CostOfSales'),
(231,23, N'Rayyan Scheme - SGA', N'مخطط الريان - مصروفات إدارية وتسويقية', N'231', N'Abstract'),
(23100,231, N'Rayyan Scheme - SGA', N'مخطط الريان - مصروفات إدارية وتسويقية', N'23100', N'SellingGeneralAndAdministration'),
(24,2, N'Yasmine Scheme', N'مخطط الياسمين', N'24', N'BusinessUnit'),
(24000,24, N'Yasmine Scheme - Cost of Sales', N'مخطط الياسمين - مصروفات مباشرة', N'24000', N'CostOfSales'),
(241,24, N'Yasmine Scheme - SGA', N'مخطط الياسمين - مصروفات إدارية وتسويقية', N'241', N'Abstract'),
(24100,241, N'Yasmine Scheme - SGA', N'مخطط الياسمين - مصروفات إدارية وتسويقية', N'24100', N'SellingGeneralAndAdministration'),
(25,2, N'Mehira Scheme', N'مخطط مهيرة', N'25', N'BusinessUnit'),
(25000,25, N'Mehira Scheme - Cost of Sales', N'مخطط مهيرة - مصروفات مباشرة', N'25000', N'CostOfSales'),
(251,25, N'Mehira Scheme - SGA', N'مخطط مهيرة - مصروفات إدارية وتسويقية', N'251', N'Abstract'),
(25100,251, N'Mehira Scheme - SGA', N'مخطط مهيرة - مصروفات إدارية وتسويقية', N'25100', N'SellingGeneralAndAdministration'),
(252,25, N'Mehira Scheme - Projects under construction', N'مخطط مهيرة - مشاريع قيد التنفيذ', N'252', N'Abstract'),
--(25200,252, N'Mehira Scheme - Shared Expenses', N'مخطط مهيرة - مصروفات مشتركة', N'25200', N'SharedExpenseControl'),
(25201,252, N'Mehira Scheme - Phase 1', N'مخطط مهيرة - المرحلة الأولى', N'25201', N'ConstructionExpenseControl'),
(25202,252, N'Mehira Scheme - Phase 2', N'مخطط مهيرة - المرحلة الثانية', N'25202', N'ConstructionExpenseControl');

-- There is already a center
UPDATE @Centers SET [Id] = (SELECT MIN([Id]) FROM dbo.Centers)
WHERE [Index] = 0

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Declarations
DECLARE @107C_SSIA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'SSIA');
DECLARE @107C_Headquarters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Headquarters');
DECLARE @107C_ConsolidatedBoardofDirectors INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Consolidated Board of Directors');
DECLARE @107C_BoardofCommissioners INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Board of Commissioners');
DECLARE @107C_DirectorateGeneralforFinancialAffairs INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Directorate General for Financial Affairs');
DECLARE @107C_FinanceDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Finance Department');
DECLARE @107C_AccountsDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Accounts Division');
DECLARE @107C_FinalAccountsDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Final Accounts Division');
DECLARE @107C_CollectionDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Collection Division');
DECLARE @107C_PaymentsDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Payments Division');
DECLARE @107C_ProcurementDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Procurement Division');
DECLARE @107C_DirectorateGeneralforHumanResourcesandAdministrativeAffairs INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Directorate General for Human Resources and Administrative Affairs');
DECLARE @107C_HumanResourcesDevelopmentDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Human Resources Development Department');
DECLARE @107C_PersonnelDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Personnel Division');
DECLARE @107C_TrainingDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Training Division');
DECLARE @107C_AdministrativeAffairsDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Administrative Affairs Department');
DECLARE @107C_ServicesDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Services Division');
DECLARE @107C_SecurityandSafetyDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Security and Safety Division');
DECLARE @107C_PublicadministrationcompaniesandRealEstateDevelopment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Public administration companies and Real Estate Development');
DECLARE @107C_DepartmentofAgriculturalInvestment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Department of Agricultural Investment');
DECLARE @107C_DepartmentofTourismInvestmentandHotels INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Department of Tourism Investment and Hotels');
DECLARE @107C_DepartmentofIndustrialInvestment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Department of Industrial Investment');
DECLARE @107C_DepartmentofMedicalInvestment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Department of Medical Investment');
DECLARE @107C_MarketingDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Marketing Department');
DECLARE @107C_CommercialMarketing INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Commercial Marketing');
DECLARE @107C_RealEstateMarketing INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Real Estate Marketing');
DECLARE @107C_RealEstateDevelopmentDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Real Estate Development Department');
DECLARE @107C_DevelopmentDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Development Division');
DECLARE @107C_ProjectsDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Projects Division');
DECLARE @107C_DirectorateGeneralforStudiesandStrategicPlanning INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Directorate General for Studies and Strategic Planning');
DECLARE @107C_StudiesandResearchDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Studies and Research Department');
DECLARE @107C_RiskDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Risk Department');
DECLARE @107C_TotalQualityDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Total Quality Department');
DECLARE @107C_DepartmentofFeasibilityStudiesandStatistics INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Department of Feasibility Studies and Statistics');
DECLARE @107C_ResearchandDevelopmentDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Research and Development Department');
DECLARE @107C_InformationTechnologyManagement INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Information Technology Management');
DECLARE @107C_TechnicalSupportandMaintenanceDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Technical Support and Maintenance Department');
DECLARE @107C_DepartmentofNetworkandInformationSecurity INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Department of Network and Information Security');
DECLARE @107C_SystemsDevelopmentandSoftwareDivision INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Systems Development and Software Division');
DECLARE @107C_DirectorateGeneralforFinancialInvestmentandsmartpartnerships INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Directorate General for Financial Investment and smart partnerships');
DECLARE @107C_BankingSystemDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Banking System Department');
DECLARE @107C_FinancialServicesDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Financial Services Department');
DECLARE @107C_InternalTradeFinanceDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Internal Trade Finance Department');
DECLARE @107C_OfficeoftheCommissionerGeneral INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Office of the Commissioner-General');
DECLARE @107C_PublicRelationsDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Public Relations Department');
DECLARE @107C_DepartmentofMedia INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Department of Media');
DECLARE @107C_ProtocolDepartment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Protocol Department');
DECLARE @107C_ManagementOfficeExecutive INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Management Office Executive');
DECLARE @107C_InternalAuditManagement INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Internal Audit Management');
DECLARE @107C_LegalCounsel INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Legal Counsel');
DECLARE @107C_SaffatTowersComplex INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Saffat Towers Complex');
DECLARE @107C_SaffatTowersDirectExpenses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Saffat Towers - Direct Expenses');
DECLARE @107C_SaffatTowersSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Saffat Towers - SGA');
DECLARE @107C_PearlComplex INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Pearl Complex');
DECLARE @107C_PearlComplexDirectExpenses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Pearl Complex - Direct Expenses');
DECLARE @107C_PearlComplexSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Pearl Complex - SGA');
DECLARE @107C_RayyanScheme INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Rayyan Scheme');
DECLARE @107C_RayyanSchemeCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Rayyan Scheme - Cost of Sales');
DECLARE @107C_RayyanSchemeSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Rayyan Scheme - SGA');
DECLARE @107C_YasmineScheme INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Yasmine Scheme');
DECLARE @107C_YasmineSchemeCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Yasmine Scheme - Cost of Sales');
DECLARE @107C_YasmineSchemeSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Yasmine Scheme - SGA');
DECLARE @107C_MehiraScheme INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Mehira Scheme');
DECLARE @107C_MehiraSchemeCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Mehira Scheme - Cost of Sales');
DECLARE @107C_MehiraSchemeSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Mehira Scheme - SGA');
DECLARE @107C_MehiraSchemePhase1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Mehira Scheme - Phase 1');
DECLARE @107C_MehiraSchemePhase2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Mehira Scheme - Phase 2');