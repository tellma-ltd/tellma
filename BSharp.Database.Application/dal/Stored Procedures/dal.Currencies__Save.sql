CREATE PROCEDURE [dal].[Currencies__Save]
	@Entities [CurrencyList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @Cash INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [Code] = N'Cash');

	IF EXISTS (SELECT Id FROM @Entities WHERE Id = CONVERT(NCHAR (3), SESSION_CONTEXT(N'FunctionalCurrencyId')))
		UPDATE [dbo].[Settings] SET SettingsVersion = NEWID(); -- The functional currency details are part of the settings

	MERGE INTO [dbo].[Currencies] AS t
	USING (
		SELECT
			[Index], [Id], [Name], [Name2], [Name3],
			[Description], [Description2], [Description3], [E]
		FROM @Entities 
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET 
			t.[Name]			= s.[Name],
			t.[Name2]			= s.[Name2],
			t.[Name3]			= s.[Name3],
			t.[Description]		= s.[Description],
			t.[Description2]	= s.[Description2],
			t.[Description3]	= s.[Description3],
			t.[E]				= s.[E],
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([Id], [Name], [Name2], [Name3], [Description], [Description2], [Description3], [E])
		VALUES (s.[Id], s.[Name], s.[Name2], s.[Name3], s.[Description], s.[Description2], s.[Description3], s.[E]);
/*
	MERGE INTO [dbo].[Resources] AS t
	USING (
		SELECT
			[Id], [Name], [Name2], [Name3],
			[Description], [Description2], [Description3]
		FROM @Entities 
	) AS s ON (t.[Code] = s.Id) AND (t.[ResourceClassificationId] = @Cash) AND (t.[DefinitionId] = N'currencies')
	WHEN MATCHED 
	THEN
		UPDATE SET 
			t.[Name]			= s.[Name],
			t.[Name2]			= s.[Name2],
			t.[Name3]			= s.[Name3],
			t.[CurrencyId]		= s.[Id],
			t.[Description]		= s.[Description],
			t.[Description2]	= s.[Description2],
			t.[Description3]	= s.[Description3],
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([Code], [DefinitionId],	[ResourceClassificationId],	[Name], [Name2], [Name3],		[Description], [Description2],	[Description3],	[Count],[CurrencyId])
		VALUES (s.[Id],  N'currencies',	@Cash,			s.[Name], s.[Name2], s.[Name3], s.[Description], s.[Description2], s.[Description3], 1,		s.[Id]);
		*/