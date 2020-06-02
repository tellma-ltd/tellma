DELETE FROM @Roles;
DELETE FROM @Members;
DELETE FROM @Permissions;

IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @Roles([Index],[Code],[Name],[Name2],[Name3],[IsPublic]) VALUES
	(0, N'Administrator', N'Administrator', N'المشرف', NULL, 0),
	(1, N'FinanceManager', N'Finance Manager', N'المدير المالي', NULL, 0),
	(2, N'GeneralManager', N'General Manager', N'المدير العام', NULL, 0),
	(3, N'Reader', N'Reader', N'صلاحية قراءة', NULL, 0),
	(4, N'AccountManager', N'Account Manager', N'مدير حساب العملاء', NULL, 0),
	(5, N'Comptroller', N'Comptroller', N'مراقب الحسابات', NULL, 0),
	(6, N'CashCustodian', N'Cashier', N'أمين الصندوق', NULL, 0),
	(7, N'AdminAffairs', N'Admin. Affairs', N'الشؤون الإدارية', NULL, 0),
	(8, N'ProductionManager', N'Production Manager', N'مدير الانتاج', NULL, 0),
	(9, N'HrManager', N'HR Manager', N'مدير الموارد البشرية', NULL, 0),
	(10, N'SalesManager', N'Sales Manager', N'مدير المبيعات', NULL, 0),
	(11, N'SalesPerson', N'Sales Person', N'مندوب مبيعات', NULL, 0),
	(12, N'InventoryCustodian', N'Inventory Custodian', N'أمين المخزون', NULL, 0),
	(99, N'Public', N'Public', N'صلاحيات عامة', NULL, 1);

	UPDATE FE
	SET FE.[Id] = BE.[Id]
	FROM @Roles FE
	JOIN dbo.Roles BE ON FE.[Code] = BE.[Code]

	INSERT INTO @Members([Index],[HeaderIndex],
	[UserId]) VALUES
	(0,0,@Jiad_akra),
	(0,1,@amtaam),
	(0,2,@mohamad_akra),
	(0,3,@amtaam),
	(0,4,@Jiad_akra),
	(1,4,@alaeldin),
	(0,5,@amtaam),
	(1,5,@aasalam),
	(0,6,@omer);
	INSERT INTO @Permissions([Index], [HeaderIndex],
	--Action: N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'State', N'All'))
		[Action],	[Criteria],			[View]) VALUES
	(0,0,N'All',	NULL,				N'all'),
	(0,1,N'All',	NULL,				N'all'),
	(0,2,N'Read',	NULL,				N'all'),
	(0,3,N'All',	N'CreatedById = Me',N'documents/revenue-recognition-vouchers'),
	(1,3,N'Update',	N'Agent/UserId = Me or (AgentId = Null and AssigneeId = Me)', -- requires specifying the safe in the header
										N'documents/cash-payment-vouchers'),
	(2,3,N'Update',	N'Agent/UserId = Me or (AgentId = Null and AssigneeId = Me)', -- requires specifying the safe in the header
										N'documents/cash-receipt-vouchers'),
	(0,4,N'All',	NULL,				N'documents/manual-journal-vouchers'),
	(1,4,N'All',	NULL,				N'documents/cash-payment-vouchers'),
	(2,4,N'All',	NULL,				N'documents/revenue-recognition-vouchers'),
	(3,4,N'Read',	NULL,				N'accounts'),
	(0,5,N'Update',	N'Agent/UserId = Me or (AgentId = Null and AssigneeId = Me)', -- requires specifying the safe in the header
										N'documents/cash-payment-vouchers'),
	(1,5,N'All',	N'Agent/UserId = Me or AssigneeId = Me',
										N'documents/cash-receipt-vouchers'),
	(2,5,N'Update', NULL,				N'contracts/suppliers'),

	(0,9,N'Read',	NULL,				N'contracts/cash-custodians'),
	(1,9,N'Read',	NULL,				N'centers'),
	(2,9,N'Read',	NULL,				N'currencies'),
	(3,9,N'Update',	N'CreatedById = Me',N'documents/cash-payment-vouchers'),
	(4,9,N'Read',	NULL,				N'resources/employee-benefits-expenses'),
	(5,9,N'Read',	NULL,				N'entry-types'),
	(6,9,N'Read',	NULL,				N'lookups/it-equipment-manufacturers'),
	(7,9,N'Read',	NULL,				N'units'),
	(8,9,N'Read',	NULL,				N'lookups/operating-systems'),
	(9,9,N'Read',	NULL,				N'centers'),
	(10,9,N'Read',	NULL,				N'resources/services-expenses'),
	(11,9,N'Read',	NULL,				N'roles'),
	(12,9,N'Read',	NULL,				N'contracts/suppliers'),
	(13,9,N'Read',	NULL,				N'users');
END

IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @Roles([Index],[Code],[Name],[Name2],[Name3],[IsPublic]) VALUES
	(0, N'Administrator', N'Administrator', NULL, NULL, 0),
	(1, N'FinanceManager', N'Finance Manager', NULL, NULL, 0),
	(2, N'GeneralManager', N'General Manager', NULL, NULL, 0),
	(3, N'Reader', N'Reader', NULL, NULL, 0),
	(4, N'AccountManager', N'Account Manager', NULL, NULL, 0),
	(5, N'Comptroller', N'Comptroller', NULL, NULL, 0),
	(6, N'CashCustodian', N'Cashier', NULL, NULL, 0),
	(7, N'AdminAffairs', N'Admin. Affairs', NULL, NULL, 0),
	(8, N'ProductionManager', N'Production Manager', NULL, NULL, 0),
	(9, N'HrManager', N'HR Manager', NULL, NULL, 0),
	(10, N'SalesManager', N'Sales Manager', NULL, NULL, 0),
	(11, N'SalesPerson', N'Sales Person', NULL, NULL, 0),
	(12, N'InventoryCustodian', N'Inventory Custodian', NULL, NULL, 0),
	(99, N'Public', N'Public', NULL, NULL, 1);

	UPDATE FE
	SET FE.[Id] = BE.[Id]
	FROM @Roles FE
	JOIN dbo.Roles BE ON FE.[Code] = BE.[Code]

	INSERT INTO @Members
	([HeaderIndex],	[Index],	[UserId])
	SELECT	0,		0,			[Id] FROM dbo.[Users] WHERE Email = N'jiad.akra@gmail.com'			UNION
	SELECT	1,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mohamad.akra@banan-it.com'	UNION
	SELECT	2,		0,			[Id] FROM dbo.[Users] WHERE Email = N'wendylulu99@gmail.com'		UNION
	SELECT	3,		0,			[Id] FROM dbo.[Users] WHERE Email = N'ahmad.akra@banan-it.com'		UNION
	SELECT	4,		0,			[Id] FROM dbo.[Users] WHERE Email = N'yisakfikadu79@gmail.com'		;

	INSERT INTO @Permissions
	--Action: N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'All'))
	([HeaderIndex],	[Index],[View],									[Action]) VALUES
	(0,				0,		N'all',									N'Read'),
	(0,				1,		N'documents/manual-journal-vouchers',	N'All'),
	(1,				0,		N'all',									N'Read'),
	(3,				0,		N'all',									N'Read');

END
--IF @DB = N'103' -- Lifan Cars, ETB, en/zh
--	INSERT INTO @Users
--	([Index],[Name],			[Name2],		[Name3],							[Email]) VALUES
--	(0,		N'Salman Al-Juhani',N'سلمان الجهني',N'萨尔曼·朱哈尼（Salman Al-Juhani)',	N'salman.aljuhani@lifan.com'),
--	(1,		N'Sami Shubaily',	N'سامي شبيلي',	N'萨米·希比利（Sami Shibili)',		N'sami.shubaily@lifan.com');

IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Roles([Index],[Code],[Name],[Name2],[Name3],[IsPublic]) VALUES
	(0, N'Administrator', N'Administrator', N'አስተዳዳሪ', NULL, 0),
	(1, N'FinanceManager', N'Finance Manager', N'የፋይናንስ አስተዳዳሪ', NULL, 0),
	(2, N'GeneralManager', N'General Manager', N'ሰላም ነው', NULL, 0),
	(3, N'Reader', N'Reader', N'አንባቢ', NULL, 0),
	(4, N'AccountManager', N'Account Manager', N'የደንበኛ መለያ አቀናባሪ', NULL, 0),
	(5, N'Comptroller', N'Comptroller', N'የመለያ ኮምፒተር', NULL, 0),
	(6, N'CashCustodian', N'Cashier', N'ገንዘብ ተቀባይ', NULL, 0),
	(7, N'AdminAffairs', N'Admin. Affairs', N'አስተዳደራዊ ጉዳዮች', NULL, 0),
	(8, N'ProductionManager', N'Production Manager', N'የምርት ሥራ አስኪያጅ', NULL, 0),
	(9, N'HrManager', N'HR Manager', N'የሰው ኃይል ሥራ አስኪያጅ', NULL, 0),
	(10, N'SalesManager', N'Sales Manager', N'የሽያጭ ሃላፊ', NULL, 0),
	(11, N'SalesPerson', N'Sales Person', N'የሽያጭ ሰው', NULL, 0),
	(12, N'InventoryCustodian', N'Inventory Custodian', N'ኢን Custስትሜንት ባለሞያ', NULL, 0),
	(99, N'Public', N'Public', N'ሕዝባዊ', NULL, 1);


	INSERT INTO @Members
	([HeaderIndex],	[Index],	[UserId])
	SELECT	0,		0,			[Id] FROM dbo.[Users] WHERE Email = N'tizitanigussie@gmail.com'		UNION
	SELECT	1,		0,			[Id] FROM dbo.[Users] WHERE Email = N'badege.kebede@gmail.com'		UNION
	SELECT	2,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mesfinwolde47@gmail.com'		UNION
	SELECT	3,		0,			[Id] FROM dbo.[Users] WHERE Email = N'ashenafi935@gmail.com'		UNION
	SELECT	4,		0,			[Id] FROM dbo.[Users] WHERE Email = N'sarabirhanuk@gmail.com'		UNION	
	SELECT	4,		1,			[Id] FROM dbo.[Users] WHERE Email = N'zewdnesh.hora@gmail.com'		UNION
	SELECT	5,		0,			[Id] FROM dbo.[Users] WHERE Email = N'tigistnegash74@gmail.com'
	INSERT INTO @Permissions
	--Action: N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'All'))
	([HeaderIndex],	[Index],[View],									[Action]) VALUES
	(0,				0,		N'all',									N'Read'),
	(0,				1,		N'documents/manual-journal-vouchers',	N'All'),
	(1,				0,		N'all',									N'Read'),
	(4,				0,		N'all',									N'Read'),
	(5,				0,		N'all',									N'Read')
	--INSERT INTO @Users
	--([Index],	[Name],						[Email]) VALUES
	--(0,			N'Badege Kebede',			N'badege.kebede@gmail.com'),
	--(1,			N'Mesfin Wolde',			N'mesfinwolde47@gmail.com'),
	--(2,			N'Ashenafi Fantahun',		N'ashenafi935@gmail.com'),
	--(3,			N'Ayelech Hora',			N'ayelech.hora@gmail.com'),
	--(4,			N'Tizita Nigussie',			N'tizitanigussie@gmail.com'),
	--(5,			N'Natnael Giragn',			N'natnaelgiragn340@gmail.com'),
	--(6,			N'Sara Birhanu',			N'sarabirhanuk@gmail.com'),
	--(7,			N'Sisay Tesfaye Bekele',	N'sisay41@yahoo.com'),
	--(8,			N'Tigist Negash',			N'tigistnegash74@gmail.com'),
	--(9,			N'Yisak Fikadu',			N'yisakfikadu79@gmail.com'),
	--(10,		N'Zewdinesh Hora',			N'zewdnesh.hora@gmail.com'),
	--(11,		N'Mestawet G/Egziyabhare',	N'mestawetezige@gmail.com'),
	--(12,		N'Belay Abagero',			N'belayabagero07@gmail.com'),
	--(13,		N'Kalkidan Asemamaw',		N'kasmamaw5@gmail.com');
END
IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Roles([Index],[Code],[Name],[Name2],[Name3],[IsPublic]) VALUES
	(0, N'Administrator', N'Administrator', N'المشرف', NULL, 0),
	(1, N'FinanceManager', N'Finance Manager', N'المدير المالي', NULL, 0),
	(2, N'GeneralManager', N'General Manager', N'المدير العام', NULL, 0),
	(3, N'Reader', N'Reader', N'صلاحية قراءة', NULL, 0),
	(4, N'AccountManager', N'Account Manager', N'مدير حساب العملاء', NULL, 0),
	(5, N'Comptroller', N'Comptroller', N'مراقب الحسابات', NULL, 0),
	(6, N'CashCustodian', N'Cashier', N'أمين الصندوق', NULL, 0),
	(7, N'AdminAffairs', N'Admin. Affairs', N'الشؤون الإدارية', NULL, 0),
	(8, N'ProductionManager', N'Production Manager', N'مدير الانتاج', NULL, 0),
	(9, N'HrManager', N'HR Manager', N'مدير الموارد البشرية', NULL, 0),
	(10, N'SalesManager', N'Sales Manager', N'مدير المبيعات', NULL, 0),
	(11, N'SalesPerson', N'Sales Person', N'مندوب مبيعات', NULL, 0),
	(12, N'InventoryCustodian', N'Inventory Custodian', N'أمين المخزون', NULL, 0),
	(99, N'Public', N'Public', N'صلاحيات عامة', NULL, 1);


	INSERT INTO @Members
	([HeaderIndex],	[Index],	[UserId])
	SELECT	0,		0,			[Id] FROM dbo.[Users] WHERE Email = N'hisham@simpex.co.sa'		UNION
	SELECT	1,		0,			[Id] FROM dbo.[Users] WHERE Email = N'nizar.kalo@simpex.co.sa'	UNION
	SELECT	2,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mahdi.mrad@simpex.co.sa'		UNION
	SELECT	3,		0,			[Id] FROM dbo.[Users] WHERE Email = N'tareq@simpex.co.sa'		UNION
	SELECT	4,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mazen.mrad@simpex.co.sa'	;
	INSERT INTO @Permissions
	--Action: N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'All'))
	([HeaderIndex],	[Index],[View],									[Action]) VALUES
	(0,				0,		N'all',									N'Read'),
	(0,				1,		N'documents/manual-journal-vouchers',	N'All'),
	(1,				0,		N'all',									N'Read'),
	(4,				0,		N'all',									N'Read'),
	(5,				0,		N'all',									N'Read');
END
--DELETE FROM @Roles WHERE [Name] IN (SELECT [Name] FROM dbo.Roles);
--DELETE FROM @Members WHERE [HeaderIndex] NOT IN (SELECT [Index] FROM @Roles);
--DELETE FROM @Permissions WHERE [HeaderIndex] NOT IN (SELECT [Index] FROM @Roles);
EXEC api.Roles__Save
	@Entities = @Roles,
	@Members = @Members,
	@Permissions = @Permissions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Permissions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;