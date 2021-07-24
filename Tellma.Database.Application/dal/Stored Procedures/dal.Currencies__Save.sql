CREATE PROCEDURE [dal].[Currencies__Save]
	@Entities [CurrencyList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @FunctionalCurrencyId NCHAR(3) = [dbo].[fn_FunctionalCurrencyId]();

	IF EXISTS (SELECT Id FROM @Entities WHERE Id = @FunctionalCurrencyId)
		UPDATE [dbo].[Settings] SET SettingsVersion = NEWID(); -- The functional currency details are part of the settings

	MERGE INTO [dbo].[Currencies] AS t
	USING (
		SELECT
			[Index], [Id], [Name], [Name2], [Name3],
			[Description], [Description2], [Description3], [NumericCode], [E]
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
			t.[NumericCode]		= s.[NumericCode],
			t.[E]				= s.[E],
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([Id], [Name], [Name2], [Name3], [Description], [Description2], [Description3], [NumericCode], [E], [CreatedById], [CreatedAt], [ModifiedById], [ModifiedAt])
		VALUES (s.[Id], s.[Name], s.[Name2], s.[Name3], s.[Description], s.[Description2], s.[Description3], s.[NumericCode], s.[E], @UserId, @Now, @UserId, @Now);

	UPDATE [dbo].[Settings] SET [SettingsVersion] = NEWID();
END;