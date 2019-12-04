CREATE FUNCTION [dbo].[InventoriesFact] (
	@CountUnitId INT,
	@MassUnitId INT,
	@VolumeUnitId INT
) RETURNS TABLE
AS
RETURN
-- TODO: rewrite to use JournalSummary instead
	SELECT
		J.[DocumentLineId],
		J.[DocumentDefinitionId],
		J.[SerialNumber],
		J.[DocumentDate],
		J.[Id],
		J.[Direction],
		J.[AgentDefinitionId],
		J.[AgentId],
		J.[ResourceId],
		J.[Count],
		J.[Mass],
		J.[Volume],
		J.[EntryClassificationId],
		J.[VoucherNumericReference],
		J.[Memo],
		J.[RelatedAgentId],
		J.[RelatedAmount],
		
		R.[DefinitionId],
		R.[Lookup1Id],
		R.[Lookup2Id]--,
		--R.[Lookup3Id],
		--R.[Lookup4Id]
	FROM dbo.[fi_NormalizedJournal](NULL, NULL, @CountUnitId, @MassUnitId, @VolumeUnitId) J
	JOIN dbo.Resources R ON J.ResourceId = R.Id
	LEFT JOIN dbo.ResourceClassifications RC ON R.ResourceClassificationId = RC.Id
	WHERE J.[ContractType] = N'NonFinancialAsset' AND J.[ResourceClassificationId] = N'Inventories'
