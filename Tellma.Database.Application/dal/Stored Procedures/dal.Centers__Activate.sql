CREATE PROCEDURE [dal].[Centers__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @BeforeBuCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [CenterType] = N'BusinessUnit' AND [IsActive] = 1);

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	MERGE INTO [dbo].[Centers] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]		= @IsActive,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;

	-- Whether there are multiple active business units is an important cached value of the settings
	DECLARE @AfterBuCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [CenterType] = N'BusinessUnit' AND [IsActive] = 1);

	IF (@BeforeBuCount <= 1 AND @AfterBuCount > 1) OR (@BeforeBuCount > 1 AND @AfterBuCount <= 1)
		UPDATE [dbo].[Settings] SET [SettingsVersion] = NEWID();
END;
