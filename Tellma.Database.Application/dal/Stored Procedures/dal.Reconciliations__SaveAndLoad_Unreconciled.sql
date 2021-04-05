CREATE PROCEDURE [dal].[Reconciliations__SaveAndLoad_Unreconciled]
	-- Save Parameters
	@AccountId					INT, 
	@RelationId					INT,
	@ExternalEntries			ExternalEntryList READONLY, -- insert/update
	@Reconciliations			ReconciliationList READONLY, -- insert
	@ReconciliationEntries		ReconciliationEntryList READONLY,--  <- insert
	@ReconciliationExternalEntries ReconciliationExternalEntryList READONLY, -- <- insert
	@DeletedExternalEntryIds	IdList READONLY,--  <- delete
	@DeletedReconcilationIds	IdList READONLY, -- <- delete
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
		@RelationId = @RelationId, 
		@ExternalEntries = @ExternalEntries, 
		@Reconciliations = @Reconciliations, 
		@ReconciliationEntries = @ReconciliationEntries, 
		@ReconciliationExternalEntries = @ReconciliationExternalEntries, 
		@DeletedExternalEntryIds = @DeletedExternalEntryIds, 
		@DeletedReconcilationIds = @DeletedReconcilationIds;
		
	-- Load
	EXEC [dal].[Reconciliation__Load_Unreconciled]
		@AccountId = @AccountId,
		@RelationId = @RelationId,
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
