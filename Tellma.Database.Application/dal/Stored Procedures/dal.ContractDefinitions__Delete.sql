CREATE PROCEDURE [dal].[ContractDefinitions__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE [dbo].[ContractDefinitions] WHERE [Id] IN (SELECT [Id] FROM @Ids);
