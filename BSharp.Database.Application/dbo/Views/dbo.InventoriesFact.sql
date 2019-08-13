CREATE FUNCTION [dbo].[InventoriesFact] (
	@MassUnitId INT,
	@CountUnitId INT
) RETURNS TABLE
AS
RETURN
-- This is used just as an example. All reports will actually be 
-- read from the TransactionViews fact table or the wrapping fi_Journal()
WITH IfrsInventoryAccounts AS (
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
		J.[ResourceId],
		J.[Mass],
		J.[Volume],
		J.[Area],
		J.[Length],
		J.[Count],
		J.[IfrsNoteId],
		J.[VoucherNumericReference],
		J.[Memo],
		J.[RelatedResourceId],
		J.[RelatedAccountId],
		J.[RelatedMoneyAmount],
		R.ResourceType,
		R.[ResourceLookup1Id],
		R.[ResourceLookup2Id],
		R.[ResourceLookup3Id],
		R.[ResourceLookup4Id]
	FROM dbo.[fi_NormalizedJournal](NULL, NULL, @MassUnitId, @CountUnitId) J
	JOIN dbo.Resources R ON J.ResourceId = R.Id
	WHERE J.IfrsAccountId IN (SELECT Id FROM IfrsInventoryAccounts);
