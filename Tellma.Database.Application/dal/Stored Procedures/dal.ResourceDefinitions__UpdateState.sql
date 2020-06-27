CREATE PROCEDURE [dal].[ResourceDefinitions__UpdateState]
	@Ids [dbo].[IdList] READONLY,
	@State NVARCHAR(50)
AS
	UPDATE [dbo].[ResourceDefinitions]
	SET [State] = @State
	WHERE [Id] IN (SELECT [Id] FROM @Ids);

	-- Notify the world to update their cache
	UPDATE [dbo].[Settings] 
	SET [DefinitionsVersion] = NEWID();