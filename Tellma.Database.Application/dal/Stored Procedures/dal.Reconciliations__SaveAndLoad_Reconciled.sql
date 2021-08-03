CREATE PROCEDURE [dal].[Reconciliations__SaveAndLoad_Reconciled]
	-- Save Parameters
	@AccountId					INT, 
	@RelationId					INT,
	@ExternalEntries			[dbo].[ExternalEntryList] READONLY, -- insert/update
	@Reconciliations			[dbo].[ReconciliationList] READONLY, -- insert
	@ReconciliationEntries		[dbo].[ReconciliationEntryList] READONLY,--  <- insert
	@ReconciliationExternalEntries [dbo].[ReconciliationExternalEntryList] READONLY, -- <- insert
	@DeletedExternalEntryIds	[dbo].[IdList] READONLY,--  <- delete
	@DeletedReconcilationIds	[dbo].[IdList] READONLY, -- <- delete
	@UserId						INT,
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
		@DeletedReconcilationIds = @DeletedReconcilationIds,
		@UserId = @UserId;

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