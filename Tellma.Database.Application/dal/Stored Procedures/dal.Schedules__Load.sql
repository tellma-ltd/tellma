CREATE PROCEDURE [dal].[Schedules__Load]
	@SchedulesVersion NVARCHAR(255) OUTPUT,
	@SettingsVersion NVARCHAR(255) OUTPUT,
	@SupportEmails NVARCHAR(255) OUTPUT
AS
BEGIN
	SELECT @SchedulesVersion = [SchedulesVersion], @SettingsVersion = [SettingsVersion], @SupportEmails = [SupportEmails] FROM [dbo].[Settings];

	SELECT [Id], [Schedule], [LastExecuted], [IsError] FROM [dbo].[NotificationTemplates] WHERE [IsDeployed] = 1 AND [Trigger] = N'Automatic';
	SELECT [Id], [Schedule], [LastExecuted], [IsError] FROM [dbo].[MessageTemplates] WHERE [IsDeployed] = 1 AND [Trigger] = N'Automatic';
END