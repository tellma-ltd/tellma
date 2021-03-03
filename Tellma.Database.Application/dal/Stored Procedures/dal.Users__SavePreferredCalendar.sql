CREATE PROCEDURE [dal].[Users__SavePreferredCalendar]
	@PreferredCalendar NCHAR(2)
AS
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	UPDATE [dbo].[Users] SET 
		[PreferredCalendar] = @PreferredCalendar, 
		[ModifiedAt] = SYSDATETIMEOFFSET(),
		[ModifiedById] = @UserId,
		[UserSettingsVersion] = NEWID()
	WHERE [Id] = @UserId AND ([PreferredCalendar] IS NULL OR [PreferredCalendar] <> @PreferredCalendar);