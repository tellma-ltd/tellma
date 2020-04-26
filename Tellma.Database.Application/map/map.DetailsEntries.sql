CREATE FUNCTION [map].[DetailsEntries] ()
RETURNS TABLE
AS
RETURN
With RU AS
(
	select ResourceId, MU.UnitType, Multiplier * BaseAmount AS Multiplier
	from dbo.ResourceUnits RU
	JOIN dbo.[Units] MU ON RU.UnitId = MU.Id
),
E AS
(
	SELECT
		E.[Id],
		E.[LineId],
		E.[CenterId],
		E.[Direction],
		E.[AccountId],
		E.[RelationId],
		E.[ContractId],
		E.[EntryTypeId],
		E.[ResourceId],
		E.[DueDate],
		E.[MonetaryValue],
		E.[CurrencyId],
		E.[Quantity],
		E.[UnitId],
		E.[Quantity] * [BaseAmount] AS NormalizedQuantity,
		MU.[UnitType],
		E.[Value],
		E.[ExternalReference],
		E.[AdditionalReference],
		E.[NotedRelationId],
		E.[NotedAgentName],
		E.[NotedAmount],
		E.[NotedDate]
	FROM
		[dbo].[Entries] E
		LEFT JOIN dbo.[Units] MU ON E.UnitId = MU.[Id]
),
-- TODO: Check performance and see if using PIVOT improves the performance
EA AS (
SELECT E.*,
	E.[NormalizedQuantity] * (SELECT [Multiplier] FROM RU WHERE [ResourceId] = E.[ResourceId] AND [UnitType] = N'Count')/
		(SELECT [Multiplier] FROM RU WHERE [ResourceId] = E.[ResourceId] AND [UnitType] = E.[UnitType]) AS [Count],
	E.[NormalizedQuantity] * (SELECT [Multiplier] FROM RU WHERE [ResourceId] = E.[ResourceId] AND [UnitType] = N'Mass')/
		(SELECT [Multiplier] FROM RU WHERE [ResourceId] = E.[ResourceId] AND [UnitType] = E.[UnitType]) AS [Mass],
	E.[NormalizedQuantity] * (SELECT [Multiplier] FROM RU WHERE [ResourceId] = E.[ResourceId] AND [UnitType] = N'Volume')/
		(SELECT [Multiplier] FROM RU WHERE [ResourceId] = E.[ResourceId] AND [UnitType] = E.[UnitType]) AS [Volume],
	E.[NormalizedQuantity] * (SELECT [Multiplier] FROM RU WHERE [ResourceId] = E.[ResourceId] AND [UnitType] = N'Time')/
		(SELECT [Multiplier] FROM RU WHERE [ResourceId] = E.[ResourceId] AND [UnitType] = E.[UnitType]) AS [Time]
FROM E
)
SELECT
		EA.*,
		EA.[Direction] * EA.[MonetaryValue]	AS [AlgebraicMonetaryValue],
		EA.[Direction] * EA.[Count] 		AS [AlgebraicCount],
		EA.[Direction] * EA.[Mass]			AS [AlgebraicMass],
		EA.[Direction] * EA.[Volume]		AS [AlgebraicVolume],
		EA.[Direction] * EA.[Time]			AS [AlgebraicTime],
		EA.[Direction] * EA.[Value]			AS [AlgebraicValue]
FROM EA