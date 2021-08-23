CREATE PROCEDURE [dal].[ResourceDefinitions__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM @Ids I JOIN [dbo].[ResourceDefinitions] D ON I.[Id] = D.[Id] WHERE D.[State] <> N'Hidden')
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	DELETE [dbo].[ResourceDefinitions]
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
END;