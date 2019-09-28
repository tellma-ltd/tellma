CREATE PROCEDURE [dal].[GLAccounts__Deprecate]
	@Ids [dbo].[IdList] READONLY,
	@IsDeprecated BIT
AS
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[GLAccounts] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.[IsDeprecated] <> @IsDeprecated)
	THEN
		UPDATE SET 
			t.[IsDeprecated]	= @IsDeprecated,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;

	MERGE INTO [dbo].[Accounts] AS t
	USING (
		SELECT [Id]
		FROM dbo.Accounts
		WHERE GLAccountId IN (SELECT [Id] FROM @Ids)
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.[IsDeprecated] <> @IsDeprecated)
	THEN
		UPDATE SET 
			t.[IsDeprecated]	= @IsDeprecated,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;