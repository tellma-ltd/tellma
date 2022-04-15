CREATE PROCEDURE [bll].[ExternalEntries__Delete]
	@AgentId INT,
	@DateAfter DATE
AS
BEGIN
	IF (SELECT COUNT(*) FROM dbo.ExternalEntries WHERE [AgentId] = @AgentId AND [PostingDate] > @DateAfter) > 500
	BEGIN
		RAISERROR(N'Too many entries. Choose a short period', 16, 1)
		RETURN
	END

	IF dal.fn_Agent__AgentDefinitionCode(@AgentId) <> N'BankAccount'
	BEGIN
		RAISERROR(N'Not a valid bank account.', 16, 1)
		RETURN
	END

	DELETE FROM dbo.ExternalEntries WHERE [AgentId] = @AgentId AND [PostingDate] > @DateAfter

	DELETE FROM dbo.ReconciliationExternalEntries
	WHERE ExternalEntryId NOT IN (SELECT Id FROM dbo.ExternalEntries)

	DELETE FROM dbo.ReconciliationEntries 
	WHERE ReconciliationId NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationExternalEntries)

	DELETE FROM dbo.Reconciliations
	WHERE [Id] NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationEntries)
	AND  [Id] NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationExternalEntries)
END