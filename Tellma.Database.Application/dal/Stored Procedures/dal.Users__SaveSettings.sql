﻿CREATE PROCEDURE [dal].[Users__SaveSettings]
	@Key NVARCHAR(255),
	@Value NVARCHAR(MAX),
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE [dbo].[Users] SET [UserSettingsVersion] = NEWID()
	WHERE [Id] = @UserId;

	IF(@Value IS NULL)
		DELETE FROM [dbo].[UserSettings] WHERE [UserId] = @UserId AND [Key] = @Key;
	ELSE
		MERGE INTO [dbo].[UserSettings] AS t
		USING (
			SELECT
				@UserId AS [UserId], @Value AS [Value], @Key AS [Key]
		) AS s ON (t.[UserId] = s.[UserId] AND t.[Key] = s.[Key])
		WHEN MATCHED 
		THEN
			UPDATE SET t.[Value] = s.[Value]

		WHEN NOT MATCHED THEN 
		INSERT ([UserId], [Key], [Value])
			VALUES (s.[UserId], s.[Key], s.[Value]);
END;



	/*************** [Algorithm] ****************
	
            If @Value is null 
                Delete from UserSettings where UserId = @UserId and Key = @Key

            If @Value is not null
                Merge the @Value in UserSettings where UserId = @UserId and Key = @Key

            If the UserSettings table has changed, update UserSettingsVersion = NEWID()
    */
