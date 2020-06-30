CREATE PROCEDURE [dal].[Centers__Activate]
	@Ids [dbo].[IdList] READONLY,
	@IsActive bit
AS
	DECLARE @BeforeSegmentCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [CenterType] = N'Segment' AND [IsActive] = 1);

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

	-- Whether there are multiple active segments is an important cached value of the settings
	DECLARE @AfterSegmentCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [CenterType] = N'Segment' AND [IsActive] = 1);

	IF (@BeforeSegmentCount <= 1 AND @AfterSegmentCount > 1) OR (@BeforeSegmentCount > 1 AND @AfterSegmentCount <= 1)
		UPDATE [dbo].[Settings] SET [SettingsVersion] = NEWID();
