CREATE PROCEDURE [dal].[Reconciliations__Insert]
	--@Reconciliations 
	@Entries IdList READONLY,
	@ExternalEntries IdList READONLY
AS
	INSERT INTO dbo.Reconciliations DEFAULT VALUES;
	DECLARE @ReconciliationId INT = SCOPE_IDENTITY();

	INSERT INTO dbo.ReconciliationEntries([ReconciliationId], [EntryId])
	SELECT @ReconciliationId, [Id] FROM @Entries

	INSERT INTO dbo.ReconciliationExternalEntries([ReconciliationId], [ExternalEntryId])
	SELECT @ReconciliationId, [Id] FROM @ExternalEntries;