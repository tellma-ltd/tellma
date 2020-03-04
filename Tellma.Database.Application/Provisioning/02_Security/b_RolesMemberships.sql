DELETE FROM @Roles;
DELETE FROM @Members;
DELETE FROM @Permissions;

IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @Roles
	([Index],	[Name],				[Code]) VALUES
	(0,			N'Comptroller',		'CMPT'),
	(1,			N'General Manager', 'GM'),
	(2,			N'Reader',			'RDR'),
	(3,			N'Account Manager',	'AM')
	;
	INSERT INTO @Members
	([HeaderIndex],	[Index],	[UserId])
	SELECT	0,		0,			[Id] FROM dbo.[Users] WHERE Email = N'jiad.akra@gmail.com'	UNION
	SELECT	1,		0,			[Id] FROM dbo.[Users] WHERE Email = N'amtaam@gmail.com'	UNION
	SELECT	2,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mohamad.akra@banan-it.com'
	SELECT	3,		0,			[Id] FROM dbo.[Users] WHERE Email = N'amtaam@gmail.com'
	INSERT INTO @Permissions
	--Action: N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'All'))
	([HeaderIndex],	[Index],[View],									[Action]) VALUES
	(0,				0,		N'all',									N'Read'),
	(0,				1,		N'documents/manual-journal-vouchers',	N'All'),
	(1,				0,		N'all',									N'Read'),
	(2,				0,		N'all',									N'Read'),
	(3,				0,		N'documents/revenue-recognition-vouchers',	N'All')
	;
END

IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @Roles
	([Index],	[Name],				[Code]) VALUES
	(0,			N'Comptroller',		'CMPT'),
	(1,			N'General Manager', 'GM'),
	(2,			N'PR Officer',		'PRO'),
	(3,			N'Reader',			'RDR'),
	(4,			N'Employee',		'EMP');

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
	INSERT INTO @Roles
	([Index],	[Name],					[Code]) VALUES
	(0,			N'Finance Manager',		'FM'),
	(1,			N'General Manager',		'GM'),
	(2,			N'Production Manager',	'PM'),
	(3,			N'Sales Manager',		'SM'),
	(4,			N'Accountant',			'AE'),
	(5,			N'Cashier',				'CS')
	;

	INSERT INTO @Members
	([HeaderIndex],	[Index],	[UserId])
	SELECT	0,		0,			[Id] FROM dbo.[Users] WHERE Email = N'tizitanigussie@gmail.com'		UNION
	SELECT	1,		0,			[Id] FROM dbo.[Users] WHERE Email = N'badege.kebede@gmail.com'		UNION
	SELECT	2,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mesfinwolde47@gmail.com'		UNION
	SELECT	3,		0,			[Id] FROM dbo.[Users] WHERE Email = N'ashenafi935@gmail.com'		UNION
	SELECT	4,		0,			[Id] FROM dbo.[Users] WHERE Email = N'sarabirhanuk@gmail.com'		UNION	
	SELECT	4,		1,			[Id] FROM dbo.[Users] WHERE Email = N'zewdnesh.hora@gmail.com'		UNION
	SELECT	5,		0,			[Id] FROM dbo.[Users] WHERE Email = N'tigistnegash74@gmail.com'
	;

	INSERT INTO @Permissions
	--Action: N'Read', N'Update', N'Delete', N'IsActive', N'IsDeprecated', N'ResendInvitationEmail', N'All'))
	([HeaderIndex],	[Index],[View],									[Action]) VALUES
	(0,				0,		N'all',									N'Read'),
	(0,				1,		N'documents/manual-journal-vouchers',	N'All'),
	(1,				0,		N'all',									N'Read'),
	(4,				0,		N'all',									N'Read'),
	(5,				0,		N'all',									N'Read')
	;

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
	INSERT INTO @Roles
	([Index],	[Name],					[Name2],			[Code]) VALUES
	(0,			N'Finance Manager',		N'المدير المالي',	'FM'),
	(1,			N'General Manager',		N'المدير العام',	'GM'),
	(2,			N'Internal Auditor',	N'المراجع الداخلي','IA'),
	(3,			N'Account Comptroller',	N'مراقب الحسايات',	'AC'),
	(4,			N'Material Control',	N'إدارة المواد',	'MC');

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

EXEC dal.Roles__Save
	@Entities = @Roles,
	@Members = @Members,
	@Permissions = @Permissions

DECLARE @1GeneralManager INT, @1Comptroller INT, @1Reader INT, @1AccountManager INT;
SELECT @1Comptroller = [Id] FROM dbo.Roles WHERE [Name] = N'Comptroller';
SELECT @1GeneralManager = [Id] FROM dbo.Roles WHERE [Name] = N'General Manager';
SELECT @1Reader = [Id] FROM dbo.Roles WHERE [Name] = N'Reader';
SELECT @1AccountManager = [Id] FROM dbo.Roles WHERE [Name] = N'Account Manager';