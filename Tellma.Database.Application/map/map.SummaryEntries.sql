CREATE FUNCTION [map].[SummaryEntries] (
	@fromDate Date = '01.01.2018',
	@toDate Date = '01.01.2019'
)
RETURNS TABLE AS
RETURN
	WITH OpeningBalances AS (
		SELECT
			E.[AccountId],
			E.[CenterId],
			E.[CurrencyId],
			E.[ResourceId],
			E.[UnitId],
			E.[CustodianId],
			E.[EntryTypeId],
			SUM(E.[AlgebraicMonetaryValue]) AS [MonetaryValue],
			SUM(E.[AlgebraicQuantity]) AS [Quantity],
			SUM(E.[AlgebraicMass]) AS [Mass],
			SUM(E.[AlgebraicValue]) AS [Value]
		FROM [map].[DetailsEntries]() E
		JOIN [dbo].[Lines] L ON E.[LineId] = L.Id
		WHERE 
			(@fromDate IS NOT NULL AND L.[PostingDate] < @fromDate)
			AND L.[State] = 4 -- TODO, return state as well
		GROUP BY E.AccountId, E.[CenterId], E.[CurrencyId], E.[ResourceId], E.[UnitId], E.[CustodianId], E.[EntryTypeId]
	),
	Movements AS (
		SELECT
			E.[AccountId],
			E.[CenterId],
			E.[CurrencyId],
			E.[ResourceId],
			E.[UnitId],
			E.[CustodianId],
			E.[EntryTypeId],
			SUM(CASE WHEN [Direction] > 0 THEN E.[MonetaryValue] ELSE 0 END) AS MonetaryValueIn,
			SUM(CASE WHEN [Direction] < 0 THEN E.[MonetaryValue] ELSE 0 END) AS MonetaryValueOut,

			SUM(CASE WHEN [Direction] > 0 THEN E.[Quantity] ELSE 0 END) AS QuantityIn,
			SUM(CASE WHEN [Direction] < 0 THEN E.[Quantity] ELSE 0 END) AS QuantityOut,
			SUM(CASE WHEN [Direction] > 0 THEN E.[Mass] ELSE 0 END) AS MassIn,
			SUM(CASE WHEN [Direction] < 0 THEN E.[Mass] ELSE 0 END) AS MassOut,

			SUM(CASE WHEN [Direction] > 0 THEN E.[Value] ELSE 0 END) AS [Debit],
			SUM(CASE WHEN [Direction] < 0 THEN E.[Value] ELSE 0 END) AS [Credit]
		FROM [map].[DetailsEntries]() E
		JOIN [dbo].[Lines] L ON E.[LineId] = L.Id
		WHERE 
			(@fromDate IS NULL OR L.[PostingDate] >= @fromDate)
		AND (@toDate IS NULL OR L.[PostingDate] <= @toDate)
		AND L.[State] = 4 -- TODO, return state as well
		GROUP BY E.AccountId, E.[CenterId], E.[CurrencyId], E.[ResourceId], E.[UnitId], E.[CustodianId], E.[EntryTypeId]
	),
	Register AS (
		SELECT
			COALESCE(OpeningBalances.AccountId, Movements.AccountId) AS [AccountId],
			COALESCE(OpeningBalances.[CenterId], Movements.[CenterId]) AS [CenterId],
			COALESCE(OpeningBalances.[CurrencyId], Movements.[CurrencyId]) AS [CurrencyId],
			COALESCE(OpeningBalances.[ResourceId], Movements.[ResourceId]) AS [ResourceId],
			COALESCE(OpeningBalances.[UnitId], Movements.[UnitId]) AS [UnitId],
			COALESCE(OpeningBalances.[CustodianId], Movements.[CustodianId]) AS [CustodianId],			
			COALESCE(OpeningBalances.[EntryTypeId], Movements.[EntryTypeId]) AS [EntryTypeId],

			ISNULL(OpeningBalances.[MonetaryValue],0) AS [OpeningMonetaryValue],
			ISNULL(OpeningBalances.[Quantity],0) AS [OpeningQuantity],
			ISNULL(OpeningBalances.[Mass],0) AS [OpeningMass],
			ISNULL(OpeningBalances.[Value],0) AS [Opening],

			ISNULL(Movements.[MonetaryValueIn],0) AS [MonetaryValueIn], ISNULL(Movements.[MonetaryValueOut],0) AS [MonetaryValueOut],
			ISNULL(Movements.[QuantityIn],0) AS [QuantityIn],	ISNULL(Movements.[QuantityOut],0) AS [QuantityOut],
			ISNULL(Movements.[MassIn],0) AS [MassIn],			ISNULL(Movements.[MassOut],0) AS [MassOut],
			ISNULL(Movements.[Debit], 0) AS [Debit],			ISNULL(Movements.[Credit], 0) AS [Credit],

			ISNULL(OpeningBalances.[MonetaryValue], 0) + ISNULL(Movements.[MonetaryValueIn], 0) - ISNULL(Movements.[MonetaryValueOut],0) AS [ClosingMonetaryValue],
			ISNULL(OpeningBalances.[Quantity], 0) + ISNULL(Movements.[QuantityIn], 0) - ISNULL(Movements.[QuantityOut],0) AS [ClosingQuantity],
			ISNULL(OpeningBalances.[Mass], 0) + ISNULL(Movements.[MassIn], 0) - ISNULL(Movements.[MassOut],0) AS [ClosingMass],
			ISNULL(OpeningBalances.[Value], 0) + ISNULL(Movements.[Debit], 0) - ISNULL(Movements.[Credit],0) AS [Closing]
		FROM OpeningBalances
		FULL OUTER JOIN Movements ON OpeningBalances.AccountId = Movements.AccountId
	)
	SELECT
		[AccountId], 
		[CenterId],
		[CurrencyId],
		[ResourceId],
		[UnitId],
		[CustodianId],
		[EntryTypeId],
		[OpeningMonetaryValue],	[MonetaryValueIn],	[MonetaryValueOut],	[ClosingMonetaryValue],
		[OpeningQuantity],		[QuantityIn],		[QuantityOut],		[ClosingQuantity],
		[OpeningMass],			[MassIn],			[MassOut],			[ClosingMass],
		[Opening],				[Debit],			[Credit],			[Closing]
	FROM Register R
;