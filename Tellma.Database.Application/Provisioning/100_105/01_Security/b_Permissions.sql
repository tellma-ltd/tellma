INSERT INTO @Roles([Index],[Id], [Code],[Name],[Name2],[Name3],[IsPublic])
SELECT [Id], [Id], [Code],[Name],[Name2],[Name3],[IsPublic]
FROM dbo.Roles;

IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @Members
	([Index],	[HeaderIndex],		[UserId]) VALUES
	(0,			@AdministratorRL,	@AdminUserId),
	(5,			@GeneralManagerRL,	@amtaam),
	(0,			@FinanceManagerRL,	@Jiad_akra),
	(0,			@ComptrollerRL,		@alaeldin),
	(0,			@AdminAffairsRL,	@omer)

	--IF @101MohamadAkra <> @AdminUserId
	--	INSERT INTO @Members([Index],[HeaderIndex],	[UserId])
	--	VALUES(10,@AdministratorRL,@101MohamadAkra);
	--IF @101AhmadAkra <> @AdminUserId
	--	INSERT INTO @Members([Index],[HeaderIndex],	[UserId])
	--	VALUES(11,@AdministratorRL,@101AhmadAkra);
END
IF @DB = N'102' -- Banan SD, USD, en
BEGIN
	INSERT INTO @Members
	([Index],	[HeaderIndex],		[UserId]) VALUES
	(0,			@AdministratorRL,	@AdminUserId);
	--(5,			@GeneralManagerRL,	@amtaam),
	--(0,			@FinanceManagerRL,	@Jiad_akra),
	--(0,			@ComptrollerRL,		@alaeldin),
	--(0,			@AdminAffairsRL,	@omer)

	--SELECT	0,		0,			[Id] FROM dbo.[Users] WHERE Email = N'jiad.akra@gmail.com'			UNION
	--SELECT	1,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mohamad.akra@banan-it.com'	UNION
	--SELECT	2,		0,			[Id] FROM dbo.[Users] WHERE Email = N'wendylulu99@gmail.com'		UNION
	--SELECT	3,		0,			[Id] FROM dbo.[Users] WHERE Email = N'ahmad.akra@banan-it.com'		UNION
	--SELECT	4,		0,			[Id] FROM dbo.[Users] WHERE Email = N'yisakfikadu79@gmail.com'		;

	--IF @102MohamadAkra <> @AdminUserId
	--	INSERT INTO @Members([Index],[HeaderIndex],	[UserId])
	--	VALUES(10,@AdministratorRL,@102MohamadAkra);
	--IF @102AhmadAkra <> @AdminUserId
	--	INSERT INTO @Members([Index],[HeaderIndex],	[UserId])
	--	VALUES(11,@AdministratorRL,@102AhmadAkra);
END
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Members
	([HeaderIndex],	[Index],	[UserId])
	SELECT	0,		0,			[Id] FROM dbo.[Users] WHERE Email = N'tizitanigussie@gmail.com'		UNION
	SELECT	1,		0,			[Id] FROM dbo.[Users] WHERE Email = N'badege.kebede@gmail.com'		UNION
	SELECT	2,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mesfinwolde47@gmail.com'		UNION
	SELECT	3,		0,			[Id] FROM dbo.[Users] WHERE Email = N'ashenafi935@gmail.com'		UNION
	SELECT	4,		0,			[Id] FROM dbo.[Users] WHERE Email = N'sarabirhanuk@gmail.com'		UNION	
	SELECT	4,		1,			[Id] FROM dbo.[Users] WHERE Email = N'zewdnesh.hora@gmail.com'		UNION
	SELECT	5,		0,			[Id] FROM dbo.[Users] WHERE Email = N'tigistnegash74@gmail.com'
END
IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Members
	([HeaderIndex],	[Index],	[UserId])
	SELECT	0,		0,			[Id] FROM dbo.[Users] WHERE Email = N'hisham@simpex.co.sa'		UNION
	SELECT	1,		0,			[Id] FROM dbo.[Users] WHERE Email = N'nizar.kalo@simpex.co.sa'	UNION
	SELECT	2,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mahdi.mrad@simpex.co.sa'		UNION
	SELECT	3,		0,			[Id] FROM dbo.[Users] WHERE Email = N'tareq@simpex.co.sa'		UNION
	SELECT	4,		0,			[Id] FROM dbo.[Users] WHERE Email = N'mazen.mrad@simpex.co.sa'	;

END


INSERT INTO @Permissions([Index], [HeaderIndex], [Id], [View], [Action], [Criteria], [Mask], [Memo])
SELECT [Id], [RoleId], [Id], [View], [Action], [Criteria], [Mask], [Memo]
FROM dbo.[Permissions];

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