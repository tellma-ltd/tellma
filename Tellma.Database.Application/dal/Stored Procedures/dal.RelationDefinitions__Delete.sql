CREATE PROCEDURE [dal].[RelationDefinitions__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM @Ids I JOIN [dbo].[RelationDefinitions] D ON I.[Id] = D.[Id] WHERE D.[State] <> N'Hidden')
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();
		
	DELETE [dbo].[RelationDefinitions]
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
END;