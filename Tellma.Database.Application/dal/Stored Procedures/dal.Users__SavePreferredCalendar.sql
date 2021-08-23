CREATE PROCEDURE [dal].[Users__SavePreferredCalendar]
	@PreferredCalendar NCHAR(2),
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[Users] SET 
		[PreferredCalendar] = @PreferredCalendar, 
		[ModifiedAt] = SYSDATETIMEOFFSET(),
		[ModifiedById] = @UserId,
		[UserSettingsVersion] = NEWID()
	WHERE [Id] = @UserId AND ([PreferredCalendar] IS NULL OR [PreferredCalendar] <> @PreferredCalendar);
END;