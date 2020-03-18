CREATE PROCEDURE [dal].[Centers__Activate]
	@Ids [dbo].[IdList] READONLY,
	@IsActive bit
AS
	DECLARE @BeforeCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE IsLeaf = 1 AND IsActive = 1);

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[Centers] AS t
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

	-- Whether there are multiple centers is an important settings value
	DECLARE @AfterCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE IsLeaf = 1 AND IsActive = 1);
	IF (@BeforeCount <= 1 AND @AfterCount > 1) OR (@BeforeCount > 1 AND @AfterCount <= 1) 
		UPDATE [dbo].[Settings] SET [SettingsVersion] = NEWID();
