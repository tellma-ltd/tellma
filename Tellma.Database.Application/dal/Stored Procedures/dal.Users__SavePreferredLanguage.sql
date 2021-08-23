CREATE PROCEDURE [dal].[Users__SavePreferredLanguage]
	@PreferredLanguage NCHAR(2),
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[Users] SET 
		[PreferredLanguage] = @PreferredLanguage, 
		[ModifiedAt] = SYSDATETIMEOFFSET(),
		[ModifiedById] = @UserId,
		[UserSettingsVersion] = NEWID()
	WHERE [Id] = @UserId AND ([PreferredLanguage] IS NULL OR [PreferredLanguage] <> @PreferredLanguage);
END;