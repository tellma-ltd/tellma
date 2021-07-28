CREATE PROCEDURE [dal].[AdminUsers__SaveSettings]
	@Key NVARCHAR(255),
	@Value NVARCHAR(MAX)
AS
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	UPDATE [dbo].[AdminUsers] SET UserSettingsVersion = NEWID();

	IF(@Value IS NULL)
		DELETE FROM [dbo].[AdminUserSettings] WHERE [AdminUserId] = @UserId AND [Key] = @Key;
	ELSE
		MERGE INTO [dbo].[AdminUserSettings] AS t
		USING (
			SELECT @UserId AS [AdminUserId], @Value AS [Value], @Key AS [Key]
		) AS s ON (t.[AdminUserId] = s.[AdminUserId] AND t.[Key] = s.[Key])
		WHEN MATCHED 
		THEN
			UPDATE SET t.[Value] = s.[Value]

		WHEN NOT MATCHED THEN 
		INSERT ([AdminUserId], [Key], [Value])
			VALUES (s.[AdminUserId], s.[Key], s.[Value]);
