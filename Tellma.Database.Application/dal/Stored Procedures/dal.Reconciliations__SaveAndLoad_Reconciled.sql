CREATE PROCEDURE [dal].[Reconciliations__SaveAndLoad_Reconciled]
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
	@FromDate					DATE,
	@ToDate						DATE,
	@FromAmount					DECIMAL (19, 4),
	@ToAmount					DECIMAL (19, 4),
	@ExternalReferenceContains	NVARCHAR (50),
	@Top						INT, 
	@Skip						INT,
	@ReconciledCount			INT OUTPUT
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
	EXEC [dal].[Reconciliation__Load_Reconciled]
		@AccountId = @AccountId,
		@RelationId = @RelationId,
		@FromDate = @FromDate,
		@ToDate = @ToDate,
		@FromAmount = @FromAmount,
		@ToAmount = @ToAmount,
		@ExternalReferenceContains = @ExternalReferenceContains,
		@Top = 	@Top,
		@Skip =	@Skip,
		@ReconciledCount = @ReconciledCount OUTPUT