CREATE PROCEDURE [dal].[ContractDefinitions__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	IF EXISTS (SELECT * FROM @Ids I JOIN [dbo].[ContractDefinitions] D ON I.[Id] = D.[Id] WHERE D.[State] <> N'Hidden')
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();
		
	DELETE [dbo].[ContractDefinitions]
	WHERE [Id] IN (SELECT [Id] FROM @Ids);