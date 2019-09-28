CREATE FUNCTION [dbo].[InventoriesFact] (
	@MassUnitId INT,
	@CountUnitId INT
) RETURNS TABLE
AS
RETURN
-- This is used just as an example. All reports will actually be 
-- read from the TransactionViews fact table or the wrapping fi_Journal()
WITH IfrsInventoryAccounts AS (
	SELECT Id FROM dbo.[AccountDefinitions]
	WHERE [Id] IN (N'TradeMerchandise', N'RawMaterials', N'FinishedGoods')
	-- TODO: Add OR GLAccountId IN GLAccountsCodeList
)
	SELECT
		J.[DocumentLineId],
		J.[DocumentDefinitionId],
		J.[SerialNumber],
		J.[DocumentDate],
		J.[Id],
		J.[Direction],
		J.[AccountId],
		J.[AccountDefinitionId],
		J.[ResponsibleActorId],
		J.[ResourceId],
		J.[Mass],
		J.[Volume],
		J.[Area],
		J.[Length],
		J.[Count],
		J.[IfrsEntryClassificationId],
		J.[VoucherNumericReference],
		J.[Memo],
		J.[RelatedResourceId],
		J.[RelatedAgentId],
		J.[RelatedMonetaryAmount],
		RC.[ResourceDefinitionId],
		R.[ResourceLookup1Id],
		R.[ResourceLookup2Id],
		R.[ResourceLookup3Id],
		R.[ResourceLookup4Id]
	FROM dbo.[fi_NormalizedJournal](NULL, NULL, @MassUnitId, @CountUnitId) J
	JOIN dbo.Resources R ON J.ResourceId = R.Id
	LEFT JOIN dbo.ResourceClassifications RC ON R.ResourceClassificationId = RC.Id
	WHERE J.[AccountDefinitionId] IN (SELECT Id FROM IfrsInventoryAccounts);
