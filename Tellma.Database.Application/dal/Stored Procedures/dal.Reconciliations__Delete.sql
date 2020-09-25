CREATE PROCEDURE [dal].[Reconciliations__Delete]
	@Reconciliations IdList READONLY
AS
	DELETE FROM dbo.Reconciliations
	WHERE [Id] IN (SELECT [Id] FROM @Reconciliations)