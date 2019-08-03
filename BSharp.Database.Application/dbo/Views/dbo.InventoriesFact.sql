CREATE VIEW [dbo].[InventoriesFact]
-- This is used just as an example. All reports will actually be 
-- read from the TransactionViews fact table or the wrapping fi_Journal()
AS
WITH IfrsInventoryAccounts
AS
(
	SELECT Id FROM dbo.[IfrsAccounts]
	WHERE [Node].IsDescendantOf(
		(SELECT [Node] FROM dbo.IfrsAccounts WHERE Id = N'Inventories')
	) = 1
)
	SELECT
			J.[DocumentLineId],
			J.[DocumentTypeId],
			J.[SerialNumber],
			J.[DocumentDate],
			J.[Id],
			J.[Direction],
			J.[AccountId],
			J.[IfrsAccountId],
			J.[ResponsibilityCenterId],
			-- J.[OperationId],
			-- J.[ProductCategoryId],
			-- J.[GeographicRegionId],
			-- J.[CustomerSegmentId],
			-- J.[TaxSegmentId],
			J.[ResourceId],
			J.[Quantity],
			J.[MoneyAmount],
			J.[NormalizedMoneyAmount],
			J.[Mass],
			J.[NormalizedMass],
			J.[Volume],
			--J.[NormalizedVolume],
			J.[Count],
			J.[NormalizedCount],
			J.[Time],
			J.[Value],
			J.[IfrsNoteId],
			J.[VoucherNumericReference],
			J.[Memo],
			J.[RelatedResourceId],
			J.[RelatedAccountId],
			J.[RelatedMoneyAmount],
			R.ResourceType,
			R.[UnitId],
			R.[ResourceLookup1Id],
			R.[ResourceLookup2Id],
			R.[ResourceLookup3Id],
			R.[ResourceLookup4Id]
	FROM dbo.[fi_JournalDetails](NULL, NULL) J
	JOIN dbo.Resources R ON J.ResourceId = R.Id
	WHERE J.IfrsAccountId IN (SELECT Id FROM IfrsInventoryAccounts);
