
DECLARE @Shareholders dbo.[AgentList];
	INSERT INTO @Banks([Index],
		[Name],									[IsRelated], [Code]) VALUES
		(16, N'Sisay Tesfaye, PLC',						0,		'O'),
		(17, N'Ethiopian Revenues and Customs Authority',0,		'T'); -- taxing
		
		
		DECLARE  @CostObjects  [dbo].[AgentList];
	 DECLARE @Sesay int, @ERCA int;
	 INSERT INTO @Organizations([Index],
		[Name],									[IsRelated], [Code]) VALUES	

	(26, N'Executive Office',						1,		'R'),
	(27, N'Production Department',					0,		'R'),
	(28, N'Sales & Marketing Department',			0,		'R'),
	(29, N'Finance Department',						0,		'R'),
	(30, N'Human Resources Department',				0,		'R'),
	(31, N'Materials & Purchasing Department',		0,		'R');

SELECT
	@Sesay = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Sisay Tesfaye, PLC'),
	@ERCA = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Ethiopian Revenues and Customs Authority');

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