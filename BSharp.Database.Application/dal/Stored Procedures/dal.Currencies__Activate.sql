CREATE PROCEDURE [dal].[Currencies__Activate]
	@Ids [dbo].[StringList] READONLY,
	@IsActive bit
AS
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[Currencies] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]		= @IsActive,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;

	WITH CurrencyResources AS
	(
		SELECT [Id] FROM dbo.Resources
		WHERE [DefinitionId] = N'monetary-resources'
		AND [ResourceClassificationId] = dbo.fn_RCCode__Id(N'Cash')
		AND [CurrencyId] IN (SELECT [Id] FROM @Ids)
	)
	MERGE INTO [dbo].[Resources] AS t
	USING (
		SELECT [Id]
		FROM CurrencyResources
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]		= @IsActive,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;