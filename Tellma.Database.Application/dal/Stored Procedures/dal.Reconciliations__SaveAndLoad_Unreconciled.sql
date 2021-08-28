CREATE PROCEDURE [dal].[Reconciliations__SaveAndLoad_Unreconciled]
	-- Save Parameters
	@AccountId					INT, 
	@AgentId					INT,
	@ExternalEntries			ExternalEntryList READONLY, -- insert/update
	@Reconciliations			ReconciliationList READONLY, -- insert
	@ReconciliationEntries		ReconciliationEntryList READONLY,--  <- insert
	@ReconciliationExternalEntries ReconciliationExternalEntryList READONLY, -- <- insert
	@DeletedExternalEntryIds	IdList READONLY,--  <- delete
	@DeletedReconcilationIds	IdList READONLY, -- <- delete
	@UserId						INT,
	-- Load Parameters
	@AsOfDate		DATE, 
	@Top			INT, 
	@Skip			INT,
	@TopExternal	INT, 
	@SkipExternal	INT,
	@EntriesBalance						DECIMAL (19,4) OUTPUT,
	@UnreconciledEntriesBalance			DECIMAL (19,4) OUTPUT,
	@UnreconciledExternalEntriesBalance	DECIMAL (19,4) OUTPUT,
	@UnreconciledEntriesCount			INT OUTPUT,
	@UnreconciledExternalEntriesCount	INT OUTPUT
AS
	-- Save
	EXEC [dal].[Reconciliations__Save]
		@AccountId = @AccountId, 
		@AgentId = @AgentId, 
		@ExternalEntries = @ExternalEntries, 
		@Reconciliations = @Reconciliations, 
		@ReconciliationEntries = @ReconciliationEntries, 
		@ReconciliationExternalEntries = @ReconciliationExternalEntries, 
		@DeletedExternalEntryIds = @DeletedExternalEntryIds, 
		@DeletedReconcilationIds = @DeletedReconcilationIds,
		@UserId = @UserId;
		
	-- Load
	EXEC [dal].[Reconciliation__Load_Unreconciled]
		@AccountId = @AccountId,
		@AgentId = @AgentId,
		@AsOfDate = @AsOfDate,
		@Top = 	@Top,
		@Skip =	@Skip,
		@TopExternal = @TopExternal, 
		@SkipExternal =	@SkipExternal,
		@EntriesBalance = @EntriesBalance OUTPUT,
		@UnreconciledEntriesBalance = @UnreconciledEntriesBalance OUTPUT,
		@UnreconciledExternalEntriesBalance = @UnreconciledExternalEntriesBalance OUTPUT,
		@UnreconciledEntriesCount = @UnreconciledEntriesCount OUTPUT,
		@UnreconciledExternalEntriesCount =	@UnreconciledExternalEntriesCount OUTPUT
