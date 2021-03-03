CREATE PROCEDURE [dal].[Users__SavePreferredLanguage]
	@PreferredLanguage NCHAR(2)
AS
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	UPDATE [dbo].[Users] SET 
		[PreferredLanguage] = @PreferredLanguage, 
		[ModifiedAt] = SYSDATETIMEOFFSET(),
		[ModifiedById] = @UserId,
		[UserSettingsVersion] = NEWID()
	WHERE [Id] = @UserId AND ([PreferredLanguage] IS NULL OR [PreferredLanguage] <> @PreferredLanguage);