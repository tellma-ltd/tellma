CREATE FUNCTION [dbo].[InventoriesFact] ()
RETURNS TABLE
AS
RETURN
-- TODO: rewrite to use JournalSummary instead
	SELECT
		J.[LineId],
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
		J.[EntryTypeId],
		J.[VoucherNumericReference],
		J.[Memo],
		J.[NotedAgentId],
		J.[NotedAmount],
		
		R.[DefinitionId],
		R.[Lookup1Id],
		R.[Lookup2Id]--,
		--R.[Lookup3Id],
		--R.[Lookup4Id]
	FROM [rpt].[Entries](NULL, NULL) J
	JOIN dbo.Resources R ON J.ResourceId = R.Id
	WHERE J.[AccountTypeId] = dbo.[fn_ATCode__Id]('TotalInventories')
