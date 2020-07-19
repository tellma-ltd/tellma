CREATE FUNCTION [map].[DetailsEntries2] (@PresentationCurrency NCHAR (3) = NULL)
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
		 IIF(RBU.[UnitType] = N'Mass', 1, R.[UnitMass]) AS [Density],
		 CASE
			WHEN @PresentationCurrency IS NULL THEN NULL
			WHEN E.[CurrencyId] = @PresentationCurrency THEN E.MonetaryValue
			ELSE E.[MonetaryValue] * ER1.[Rate] / ER2.[Rate]
		END AS [PValue]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.LineDefinitions LD ON L.[DefinitionId] = LD.[Id] 
	LEFT JOIN [map].[ExchangeRates]() ER1 ON E.CurrencyId = ER1.CurrencyId AND L.PostingDate >= ER1.ValidAsOf AND L.PostingDate < ER1.ValidTill
	LEFT JOIN [map].[ExchangeRates]() ER2 ON ER2.CurrencyId = @PresentationCurrency AND L.PostingDate >= ER2.ValidAsOf AND L.PostingDate < ER2.ValidTill
	LEFT JOIN dbo.Resources R ON E.ResourceId = R.[Id]
	LEFT JOIN dbo.Units EU ON E.UnitId = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
	WHERE (LD.[Code] <> N'CurrencyTranslation' OR E.CurrencyId = @PresentationCurrency)
)
SELECT
		EB.*,
		EB.[Direction] * EB.[MonetaryValue]				AS [AlgebraicMonetaryValue],
	-	EB.[Direction] * EB.[MonetaryValue]				AS [NegativeAlgebraicMonetaryValue],
		EB.[Direction] * EB.[BaseQuantity]				AS [AlgebraicQuantity],
	-	EB.[Direction] * EB.[BaseQuantity]				AS [NegativeAlgebraicQuantity],
		EB.[BaseQuantity] * [Density]					AS [Mass],
		EB.[Direction] * EB.[BaseQuantity] * [Density]	AS [AlgebraicMass],
		EB.[Direction] * EB.[PValue]					AS [AlgebraicValue],
	-	EB.[Direction] * EB.[PValue]					AS [NegativeAlgebraicValue]
FROM EB
GO