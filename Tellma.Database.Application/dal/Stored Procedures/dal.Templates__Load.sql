CREATE PROCEDURE [dal].[Templates__Load]
	@EmailTemplateIds [dbo].[IdList] READONLY,
	@MessageTemplateIds [dbo].[IdList] READONLY,
	@SchedulesVersion NVARCHAR(255) OUTPUT,
	@SettingsVersion NVARCHAR(255) OUTPUT,
	@SupportEmails NVARCHAR(255) OUTPUT
AS
BEGIN
	SELECT @SchedulesVersion = [SchedulesVersion], @SettingsVersion = [SettingsVersion], @SupportEmails = [SupportEmails] FROM [dbo].[Settings];
	
	-- TODO: Return the full templates
	SELECT [Id], [Schedule], [LastExecuted] FROM [dbo].[NotificationTemplates] WHERE [IsDeployed] = 1 AND [Trigger] = N'Automatic';

	-- MessageTemplates
	SELECT * FROM [dbo].[MessageTemplates] 
	WHERE [Id] IN (SELECT [Id] FROM @MessageTemplateIds);

	-- MessageTemplateParameters
	SELECT * FROM [dbo].[MessageTemplateParameters] 
	WHERE [MessageTemplateId] IN (SELECT [Id] FROM @MessageTemplateIds)

	-- Users
	SELECT * FROM [dbo].[Users]
	WHERE [Id] IN (
		SELECT [UserId] FROM [dbo].[MessageTemplateSubscribers] 
		WHERE [MessageTemplateId] IN (SELECT [Id] FROM @MessageTemplateIds)
	)

	-- MessageTemplateSubscribers
	SELECT * FROM [dbo].[MessageTemplateSubscribers] 
	WHERE [MessageTemplateId] IN (SELECT [Id] FROM @MessageTemplateIds)
END