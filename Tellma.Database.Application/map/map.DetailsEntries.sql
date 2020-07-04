CREATE FUNCTION [map].[DetailsEntries] ()
	RETURNS TABLE
	AS
	RETURN
With EA AS (
SELECT *--[AccountId], [ResourceId], [Quantity], [UnitId], [Count], [Mass], [Volume], [Length]
FROM
(
	SELECT
		E.*,
		CAST(E.[Quantity] * -- Quantity in E.UnitId
			UE.[BaseAmount] / UE.[UnitAmount] -- Quantity in Standard Unit of that type
		 --*	(RUR.[UnitAmount]) / (RUR.[BaseAmount]) As [StandardAmount],-- Quantity in Standard Unit of all compatible unit types
		 / (RUR.[Multiplier]) AS DECIMAL (19, 4)) As [StandardAmount],
		U2.[UnitType]
	FROM dbo.Entries E
	LEFT JOIN dbo.Units UE ON E.UnitId = UE.[Id]
	LEFT JOIN dbo.ResourceUnits RUR ON RUR.[ResourceId] = E.[ResourceId]
	LEFT JOIN dbo.Units U2 ON RUR.[UnitId] = U2.[Id]
) AS SourceTable
PIVOT
(
	SUM([StandardAmount])
	FOR UnitType IN ([Count], [Mass], [Volume], [Length], [Time])
) AS PivotTable
)
SELECT
		EA.*,
		EA.[Direction] * EA.[MonetaryValue]		AS [AlgebraicMonetaryValue],
		EA.[Direction] * -EA.[MonetaryValue]	AS [NegativeAlgebraicMonetaryValue],
		EA.[Direction] * EA.[Quantity]			AS [AlgebraicQuantity],
		EA.[Direction] * -EA.[Quantity]			AS [NegativeAlgebraicQuantity],
		EA.[Direction] * EA.[Count] 			AS [AlgebraicCount],
		EA.[Direction] * EA.[Mass]				AS [AlgebraicMass],
		EA.[Direction] * EA.[Volume]			AS [AlgebraicVolume],
		EA.[Direction] * EA.[Length]			AS [AlgebraicLength],
		EA.[Direction] * EA.[Time]				AS [AlgebraicTime],
		EA.[Direction] * EA.[Value]				AS [AlgebraicValue],
		EA.[Direction] * -EA.[Value]			AS [NegativeAlgebraicValue]
FROM EA