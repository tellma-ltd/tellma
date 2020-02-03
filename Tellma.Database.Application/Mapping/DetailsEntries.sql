CREATE FUNCTION [map].[DetailsEntries] ()
RETURNS TABLE
AS
RETURN
	SELECT
		E.[Id],
		E.[LineId],
		E.[ResponsibilityCenterId],
		E.[Direction],
		E.[AccountId],
		--E.[AccountIdentifier]
		E.[AgentId],
		E.[EntryTypeId],
		E.[ResourceId],
		--E.[ResourceIdentifier],
		E.[DueDate],

		E.[MonetaryValue],
		E.[Direction] * E.[MonetaryValue] AS [AlgebraicMonetaryValue],
		E.[CurrencyId],

		E.[Count],
		E.[Direction] * E.[Count] AS [AlgebraicCount],

		E.[Mass],
		E.[Direction] * E.[Mass] AS [AlgebraicMass],

		E.[Volume],
		E.[Direction] * E.[Volume] AS [AlgebraicVolume],
		
		E.[Time],
		E.[Direction] * E.[Time] AS [AlgebraicTime],

		E.[Value],
		E.[Direction] * E.[Value] AS [AlgebraicValue],
		
		E.[ExternalReference],
		E.[AdditionalReference],
		E.[NotedAgentId],
		E.[NotedAgentName],
		E.[NotedAmount],
		E.[NotedDate]
	FROM
		[dbo].[Entries] E