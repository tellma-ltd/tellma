CREATE PROCEDURE [dal].[AgentDefinitions__UpdateState]
	@Ids [dbo].[IndexedIdList] READONLY,
	@State NVARCHAR(50),
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[AgentDefinitions]
	SET
		[State] = @State,
		[SavedById] = @UserId
	WHERE [Id] IN (SELECT [Id] FROM @Ids);

	-- Notify the world to update their cache
	UPDATE [dbo].[Settings] 
	SET [DefinitionsVersion] = NEWID();
END;