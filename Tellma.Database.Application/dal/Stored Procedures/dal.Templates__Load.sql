CREATE PROCEDURE [dal].[Templates__Load]
	@EmailTemplateIds [dbo].[IdList] READONLY,
	@MessageTemplateIds [dbo].[IdList] READONLY,
	@SchedulesVersion NVARCHAR(255) OUTPUT,
	@SettingsVersion NVARCHAR(255) OUTPUT,
	@SupportEmails NVARCHAR(255) OUTPUT
AS
BEGIN
	SELECT @SchedulesVersion = [SchedulesVersion], @SettingsVersion = [SettingsVersion], @SupportEmails = [SupportEmails] FROM [dbo].[Settings];
	
	-------------- EmailTemplates	
	SELECT * FROM [dbo].[EmailTemplates] 
	WHERE [Id] IN (SELECT [Id] FROM @EmailTemplateIds);
	
	-- EmailTemplateParameters
	SELECT * FROM [dbo].[EmailTemplateParameters] 
	WHERE [EmailTemplateId] IN (SELECT [Id] FROM @EmailTemplateIds)

	-- Users
	SELECT * FROM [dbo].[Users]
	WHERE [Id] IN (
		SELECT [UserId] FROM [dbo].[EmailTemplateSubscribers] 
		WHERE [EmailTemplateId] IN (SELECT [Id] FROM @EmailTemplateIds)
	)

	-- EmailTemplateSubscribers
	SELECT * FROM [dbo].[EmailTemplateSubscribers] 
	WHERE [EmailTemplateId] IN (SELECT [Id] FROM @EmailTemplateIds)

	-- PrintingTemplates
	SELECT * FROM [dbo].[PrintingTemplates]
	WHERE [Id] IN (
		SELECT [PrintingTemplateId] FROM [dbo].[EmailTemplateAttachments] 
		WHERE [EmailTemplateId] IN (SELECT [Id] FROM @EmailTemplateIds)
	)
		
	-- PrintingTemplateParameters
	SELECT * FROM [dbo].[PrintingTemplateParameters] 
	WHERE [PrintingTemplateId] IN (
			SELECT [Id] FROM [dbo].[PrintingTemplates]
			WHERE [Id] IN (
				SELECT [PrintingTemplateId] FROM [dbo].[EmailTemplateAttachments] 
				WHERE [EmailTemplateId] IN (SELECT [Id] FROM @EmailTemplateIds)
			)
	)
	
	-- EmailTemplateAttachments
	SELECT * FROM [dbo].[EmailTemplateAttachments] 
	WHERE [EmailTemplateId] IN (SELECT [Id] FROM @EmailTemplateIds)

	-------------- MessageTemplates
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