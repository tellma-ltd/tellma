CREATE PROCEDURE [dal].[LineDefinitions__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	EXEC [wiz].[SpuriousTabHeaders__Delete];

	DELETE FROM [dbo].[LineDefinitions] 
	WHERE [Id] IN (SELECT Id FROM @Ids);
	
	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();
END;
GO