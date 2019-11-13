CREATE FUNCTION [dbo].[InventoriesFact] (
	@MassUnitId INT,
	@CountUnitId INT
) RETURNS TABLE
AS
RETURN
-- TODO: rewrite to use JournalSummary instead
WITH IfrsInventoryAccounts AS (
	SELECT Id FROM dbo.[AccountTypes]
	WHERE [Id] IN (N'TradeMerchandise', N'RawMaterials', N'FinishedGoods')
)
	SELECT
		J.[DocumentLineId],
		J.[DocumentDefinitionId],
		J.[SerialNumber],
		J.[DocumentDate],
		J.[Id],
		J.[Direction],
		J.[AccountId],
		J.[ResponsibilityCenterId],
		J.[ResourceId],
		J.[Mass],
		J.[Volume],
		J.[Area],
		J.[Length],
		J.[Count],
		J.[EntryTypeId],
		J.[VoucherNumericReference],
		J.[Memo],
		J.[RelatedResourceId],
		J.[RelatedAgentId],
		J.[RelatedMonetaryValue],
		R.[ResourceDefinitionId],
		R.[Lookup1Id],
		R.[Lookup2Id],
		R.[Lookup3Id],
		R.[Lookup4Id]
	FROM dbo.[fi_NormalizedJournal](NULL, NULL, @MassUnitId, @CountUnitId) J
	JOIN dbo.Resources R ON J.ResourceId = R.Id
	LEFT JOIN dbo.ResourceClassifications RC ON R.ResourceClassificationId = RC.Id
	WHERE J.[AccountTypeId] = N'Inventory'
