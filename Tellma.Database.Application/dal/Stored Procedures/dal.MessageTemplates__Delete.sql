CREATE PROCEDURE [dal].[MessageTemplates__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;
			
	-- IF there are changes to the schedules, signal the scheduler 
	IF (EXISTS (SELECT * FROM [dbo].[MessageTemplates] O WHERE [Id] IN (SELECT [Id] FROM @Ids) AND O.[IsDeployed] = 1 AND O.[Trigger] = N'Automatic')) -- Deleted matching template
		UPDATE [dbo].[Settings] SET [SchedulesVersion] = NEWID();

	DELETE FROM [dbo].[MessageTemplates] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
	
	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();
END;