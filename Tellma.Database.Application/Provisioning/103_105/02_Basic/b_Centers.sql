IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
			[Name],						[Code], [CenterType]) VALUES
	(0,NULL,N'Walia Steel',				N'0',	N'Abstract'),

	(1,0,	N'Headquarters',			N'1',	N'BusinessUnit'),
	(100,1,	N'Common',					N'100',	N'SellingGeneralAndAdministration'), -- Badege
	(110,1,	N'Exec Office',				N'110',	N'SellingGeneralAndAdministration'), -- Badege
	(120,1,	N'Finance Dept',			N'120',	N'SellingGeneralAndAdministration'), -- Tizita

	(13,1,	N'Marketing and Sales',		N'13',	N'Abstract'),
	(130,13,N'Marketing and Sales Mgmt',N'130',	N'SellingGeneralAndAdministration'), -- Ashenafi
	(131,13,N'AG Office',				N'131',	N'SellingGeneralAndAdministration'), -- Ashenafi
	(132,13,N'Bole Office',				N'132',	N'SellingGeneralAndAdministration'), -- Ashenafi

	(14,1,	N'Service Centers',			N'14',	N'Abstract'), -- reallocate to O/H of depts based on cited basis
	(141,14,N'HR Dept',					N'141',	N'SharedExpenseControl'), -- Belay, by number of employees
	(142,14,N'Cafeteria',				N'142',	N'SharedExpenseControl'), -- Belay, by number of employees
	(143,14,N'Maintenance Dept',		N'143',	N'SharedExpenseControl'), -- Girma, by number of maintenance requests or by Direct Hours
	(144,14,N'Materials',				N'144',	N'SharedExpenseControl'), -- Ayelech, by number of Purchase Orders

	(2,0,	N'Steel',					N'2',	N'BusinessUnit'),
	(200,2,	N'Steel - Sales',			N'200',	N'CostOfSales'),
	(21,2,	N'Production',				N'21',	N'Abstract'),
	(210,21,N'Production Management',	N'210',	N'WorkInProgressExpendituresControl'),
	(211,21,N'Slitting Dept',			N'211',	N'WorkInProgressExpendituresControl'),
	(212,21,N'HSP Dept',				N'212',	N'WorkInProgressExpendituresControl'),
	(213,21,N'Cut to Size Dept',		N'213',	N'WorkInProgressExpendituresControl'),	

	(3,0,	N'Other Income',			N'3',	N'Abstract'), -- 
	(301,3,	N'T/H Bldg',				N'301',	N'BusinessUnit'), -- Bldg Manager
	(302,3,	N'Coffee',					N'302',	N'BusinessUnit'), -- Gadissa
	(399,3,	N'Misc.',					N'399',	N'BusinessUnit'); -- Gadissa


END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
				[Name],						[Name2],					[Code],[CenterType]) VALUES
	(0,NULL,	N'Simpex',					N'سيمبكس',					N'0',	N'Abstract'),
	(1,NULL,	N'Headquarters',			N'الرئاسة',					N'1',	N'BusinessUnit'),
	(10,1,		N'Common Expenses',			N'المصروفات العمومية',		N'10',	N'SellingGeneralAndAdministration'),
	(11,1,		N'Exec. Office',			N'المكتب التنفيذي',		N'11',	N'SellingGeneralAndAdministration'),
	(12,1,		N'Finance',					N'الإدارة المالية',			N'12',	N'SellingGeneralAndAdministration'),
	(13,1,		N'Legal',					N'الشؤون القانونية',		N'13',	N'SellingGeneralAndAdministration'),
	(14,1,		N'Human Resources',			N'الموارد البشرية',		N'14',	N'SharedExpenseControl'),
	(15,1,		N'IT',						N'تقنية المعلومات',		N'15',	N'SharedExpenseControl'),
	(2,0,		N'Branches',				N'الفروع',					N'2',	N'Abstract'),
	(21,2,		N'Jeddah Branch',			N'فرع جدة',					N'21',	N'BusinessUnit'),
	(22,2,		N'Riyadh Branch',			N'فرع الرياض',				N'22',	N'BusinessUnit'),
	(23,2,		N'Dammam Branch',			N'فرع الدمام',				N'23',	N'BusinessUnit')
END

INSERT INTO @ValidationErrors
EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@UserId = @AdminUserId;
	
IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'Centers: Error Inserting'
	GOTO Err_Label;
END;