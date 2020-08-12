CREATE PROCEDURE [dal].[DocumentDefinitions__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	IF EXISTS (SELECT * FROM @Ids I JOIN [dbo].[DocumentDefinitions] D ON I.[Id] = D.[Id] WHERE D.[State] <> N'Hidden')
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();
		
	DELETE [dbo].[DocumentDefinitions]
	WHERE [Id] IN (SELECT [Id] FROM @Ids);