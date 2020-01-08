
DECLARE @Owners dbo.[AgentList];
IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @Owners
	([Index], [Name]) VALUES
	(0,		N'Tom Hurton'),
	(1,		N'Jeff Bezos'),
	(2,		N'Warren Buffet');
ELSE IF @DB = N'101' -- Banan SD, USD, en
	Print N''
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @Owners
	([Index], [Name]) VALUES
	(0,		N'Mohamad Akra'),
	(1,		N'Ahmad Akra');
ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
	Print N''
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Owners
	([Index], [Name]) VALUES
	(0,		N'Sisay Tesfaye');

EXEC [api].[Agents__Save]
	@DefinitionId = N'owners',
	@Entities = @Owners,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Owners: Inserting'
	GOTO Err_Label;
END;

	 /*
BEGIN -- Users
	IF NOT EXISTS(SELECT * FROM [dbo].[Users])
	INSERT INTO [dbo].[Users]([Id], [Name], [AgentId]) VALUES
	(N'system@banan-it.com', N'B#', NULL),
	(N'mohamad.akra@banan-it.com', N'Mohamad Akra', @MohamadAkra),
	(N'ahmad.akra@banan-it.com', N'Ahmad Akra', @AhmadAkra),
	(N'badegek@gmail.com', N'Badege', @BadegeKebede),
	(N'mintewelde00@gmail.com', N'Tizita', @TizitaNigussie),
	(N'ashenafi935@gmail.com', N'Ashenafi', @Ashenafi),
	(N'yisak.tegene@gmail.com', N'Yisak', @YisakTegene),
	(N'zewdnesh.hora@gmail.com', N'Zewdinesh Hora', @ZewdineshHora),
	(N'tigistnegash74@gmail.com', N'Tigist', @TigistNegash),
	(N'roman.zen12@gmail.com', N'Roman', @RomanZenebe),
	(N'mestawetezige@gmail.com', N'Mestawet', @Mestawet),
	(N'ayelech.hora@gmail.com', N'Ayelech', @AyelechHora),
	(N'info@banan-it.com', N'Banan IT', NULL)
END

*/