CREATE FUNCTION [map].[DetailsEntries] ()
	RETURNS TABLE
	AS
	RETURN
With EB AS ( -- TODO: Test with Resources having both pure and non pure units
	SELECT
		E.*,
		IIF(EU.UnitType = N'Pure',
			E.[Quantity],
			CAST(
				E.[Quantity] -- Quantity in E.UnitId
			*	EU.[BaseAmount] / EU.[UnitAmount] -- Quantity in Standard Unit of that type
			*	RBU.[UnitAmount] / RBU.[BaseAmount]
				AS DECIMAL (19,4)
			)
		) As [BaseQuantity],-- Quantity in Base unit of that resource
		IIF(RBU.[UnitType] = N'Mass', RBU.[BaseAmount] / RBU.[UnitAmount] , R.[UnitMass]) AS [Density]
	FROM dbo.Entries E
	LEFT JOIN dbo.[Resources] R ON E.ResourceId = R.[Id]
	LEFT JOIN dbo.Units EU ON E.UnitId = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
)
SELECT
	EB.*,
	EB.[Direction] * EB.[MonetaryValue]				AS [AlgebraicMonetaryValue],
-	EB.[Direction] * EB.[MonetaryValue]				AS [NegativeAlgebraicMonetaryValue],
	EB.[Direction] * EB.[BaseQuantity]				AS [AlgebraicQuantity],
-	EB.[Direction] * EB.[BaseQuantity]				AS [NegativeAlgebraicQuantity],
	EB.[BaseQuantity] * [Density]					AS [Mass],
	EB.[Direction] * EB.[BaseQuantity] * [Density]	AS [AlgebraicMass],
	EB.[Direction] * EB.[Value]						AS [AlgebraicValue],
-	EB.[Direction] * EB.[Value]						AS [NegativeAlgebraicValue],

	EB.[Direction] * EB.[RValue]					AS [AlgebraicRestatedValue],
-	EB.[Direction] * EB.[RValue]					AS [NegativeAlgebraicRestatedValue],
	EB.[Direction] * EB.[PValue]					AS [AlgebraicPresentationValue],
-	EB.[Direction] * EB.[PValue]					AS [NegativeAlgebraicPresentationValue],

	IIF(EB.[BaseQuantity] = 0, 0, EB.[MonetaryValue] / EB.[BaseQuantity]) AS MonetaryValuePerUnit,
	IIF(EB.[BaseQuantity] = 0, 0, EB.[Value] / EB.[BaseQuantity]) AS ValuePerUnit
FROM EB