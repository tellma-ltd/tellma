CREATE PROCEDURE [dal].[AccountClassifications__Activate]
	@Ids [dbo].[IdList] READONLY,
	@IsActive BIT
AS
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[AccountClassifications] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.[IsActive] <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]	= @IsActive,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;

	IF @IsActive = 0
	MERGE INTO [dbo].[Accounts] AS t
	USING (
		SELECT [Id]
		FROM dbo.Accounts
		WHERE [ClassificationId] IN (SELECT [Id] FROM @Ids)
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.[IsDeprecated] = 0)
	THEN
		UPDATE SET 
			t.[IsDeprecated]	= 1,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;