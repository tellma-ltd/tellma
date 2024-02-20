SET NOCOUNT ON;

-- Change the color to grey, and update versions
UPDATE [dbo].[Settings] SET 
	[BrandColor] = N'#5c5c5c';

-- Change tenant name to indicate clone
UPDATE [dbo].[Settings] SET 
	[ShortCompanyName] = N'Clone of ' + [ShortCompanyName],
	[ShortCompanyName2] = N'Clone of ' + [ShortCompanyName2],
	[ShortCompanyName3] = N'Clone of ' + [ShortCompanyName3];
	
-- Reset ZATCA Settings
UPDATE [dbo].[Settings] SET 
	[ZatcaEncryptedPrivateKey] = NULL,
	[ZatcaEncryptedSecret] = NULL,
	[ZatcaEncryptedSecurityToken] = NULL,
	[ZatcaEncryptionKeyIndex] = 0,
	[ZatcaUseSandbox] = 1;

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
DELETE FROM [dbo].[ResourceAttachments]

-- Delete email and SMS histories
DELETE FROM [dbo].[EmailAttachments]
DELETE FROM [dbo].[Emails]
DELETE FROM [dbo].[EmailCommands]
DELETE FROM [dbo].[Messages]
DELETE FROM [dbo].[MessageCommands]

-- De-link all ZATCA invoices from their attachments
UPDATE [dbo].[Documents] SET
	[ZatcaState] = NULL,
	[ZatcaError] = NULL,
	[ZatcaSerialNumber] = NULL,
	[ZatcaHash] = NULL,
	[ZatcaUuid] = NULL;

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
