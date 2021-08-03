CREATE PROCEDURE [dal].[DocumentDefinitions__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM @Ids I JOIN [dbo].[DocumentDefinitions] D ON I.[Id] = D.[Id] WHERE D.[State] <> N'Hidden')
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();
	
	DELETE FROM [dbo].[DocumentDefinitionLineDefinitions]
	WHERE DocumentDefinitionId IN (SELECT [Id] FROM @Ids);
	
	DELETE FROM [dbo].[DocumentDefinitions]
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
END;