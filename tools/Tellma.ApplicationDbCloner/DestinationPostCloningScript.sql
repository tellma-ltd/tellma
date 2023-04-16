SET NOCOUNT ON;

-- Change the color to grey, and update versions
UPDATE [dbo].[Settings] SET 
	[BrandColor] = N'#5c5c5c';

-- Change tenant name to indicate clone
UPDATE [dbo].[Settings] SET 
	[ShortCompanyName] = IIF([PrimaryLanguageId] = N'en', N'Clone of ' + [ShortCompanyName], N'(✂️) ' + [ShortCompanyName]),
	[ShortCompanyName2] = IIF([SecondaryLanguageId] = N'en', N'Clone of ' + [ShortCompanyName2], N'(✂️) ' + [ShortCompanyName2]),
	[ShortCompanyName3] = IIF([TernaryLanguageId] = N'en', N'Clone of ' + [ShortCompanyName3], N'(✂️) ' + [ShortCompanyName3]);

-- Change all Users to New, and update versions
UPDATE [dbo].[Users] SET 
	[ExternalId] = IIF([IsService] = 1, [ExternalId], NULL), 
	[InvitedAt] = NULL, 
	[LastAccess] = NULL,
	[LastInboxCheck] = NULL,
	[LastNotificationsCheck] = NULL;

-- Delete all image references and attachment metadata
UPDATE [dbo].[Users] SET [ImageId] = NULL;
UPDATE [dbo].[Agents] SET [ImageId] = NULL
UPDATE [dbo].[Resources] SET [ImageId] = NULL
DELETE FROM [dbo].[Attachments]
DELETE FROM [dbo].[AgentAttachments]

-- Delete email and SMS histories
DELETE FROM [dbo].[EmailAttachments]
DELETE FROM [dbo].[Emails]
DELETE FROM [dbo].[EmailCommands]
DELETE FROM [dbo].[Messages]
DELETE FROM [dbo].[MessageCommands]

-- Disable all automatic notifications
UPDATE [dbo].[EmailTemplates] SET [IsDeployed] = 0 WHERE [Trigger] = N'Automatic';
UPDATE [dbo].[MessageTemplates] SET [IsDeployed] = 0 WHERE [Trigger] = N'Automatic';

-- Update versions
UPDATE [dbo].[Settings] SET 
	[SettingsVersion] = NEWID(), 
	[DefinitionsVersion] = NEWID();

UPDATE [dbo].[Users] SET
	[PermissionsVersion] = NEWID(),
	[UserSettingsVersion] = NEWID();
