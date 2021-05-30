CREATE PROCEDURE [dal].[InventoriesInTransit_Credit__Update]
@FromDate DATE = N'2020-07-08'
AS
DECLARE @E SMALLINT = (SELECT [E] FROM dbo.Currencies WHERE [Id] = dbo.fn_FunctionalCurrencyId());

-- For each LC center (also duplicated as custody)
-- For each resource
--	Sum all the Quantities debited to IIT account (from past transactions or from future purchases)
--	Sum all the values debited to IIT account (from past transactions or from future Expense capitalization)
--	Compute Sum of Value / Sum of Quantity
-- For each LD involving IIT
--	Update the credit entry with the new average price, starting from date
--	Update the related debit entry as well.
WITH ScalingFactors AS (
	SELECT TQ.[AccountId], TQ.[CenterId], TQ.[CustodyId], TQ.[ResourceId],-- AlgebraicQuantity, AlgebraicMonetaryValue
			TQ.[AMVPU], TQ.[AVPU]
	FROM (
		-- Collect all values: past, present, and future
		SELECT E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId],
			(SUM(E.[Direction] * E.[MonetaryValue]) / SUM(E.[Direction] * E.[BaseQuantity])) AS AMVPU, (SUM(E.[Direction] * E.[Value]) / SUM(E.[Direction] * E.[BaseQuantity])) AS AVPU
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
		JOIN dbo.EntryTypes ET ON E.[EntryTypeId] = ET.[Id]
		WHERE L.[State] = 4
		AND AC.[Concept] = N'CurrentInventoriesInTransit'
		AND (
			ET.[Concept] IN (
				N'AdditionsFromPurchasesInventoriesExtension',
				N'IncreaseThroughExpenseCapitalizationInventoriesExtension'
			) OR
			[Direction] = -1 AND L.[PostingDate] < @FromDate
		)
		GROUP BY E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId]
		HAVING SUM(E.[Direction] * E.[BaseQuantity]) <> 0
	) TQ
	JOIN (
		-- keep those who exhibit credit entries after a certain date
		SELECT E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId]
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.LineDefinitions LD ON L.[DefinitionId] = LD.[Id]
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
		JOIN dbo.EntryTypes ET ON E.[EntryTypeId] = ET.[Id]
		WHERE L.[State] = 4
		AND LD.[Code] <> N'ManualLine'
		AND E.[Direction] = -1 
		AND AC.[Concept] = N'CurrentInventoriesInTransit'
		AND L.[PostingDate] >= @FromDate
		GROUP BY E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId]
	) TC ON TQ.[AccountId] = TC.[AccountId]
		AND TQ.[CenterId] = TC.[CenterId]
		AND TQ.[CustodyId] = TC.[CustodyId]
		AND TQ.[ResourceId] = TC.[ResourceId]
),
AffectedEntries AS (
	SELECT E.[LineId], E.[Index], SF.[AMVPU], SF.[AVPU]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.LineDefinitions LD ON L.[DefinitionId] = LD.[Id]
	JOIN ScalingFactors SF ON E.[AccountId] = SF.[AccountId]
		AND E.[CenterId] = SF.[CenterId]
		AND E.[CustodyId] = SF.[CustodyId]
		AND E.[ResourceId] = SF.[ResourceId]
	WHERE LD.[Code] <> N'ManualLine'
	AND E.[Direction] = -1
	AND L.[PostingDate] >= @FromDate
)
--SELECT E.[LineId], E.[Index], E.[MonetaryValue], ROUND(AE.[AMVPU] * E.[BaseQuantity], C.E)  AS NewMonetaryValue,
--		E.[Value], ROUND(AE.[AVPU] * E.[BaseQuantity], @E) AS NewValue
--		, E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId], E.[Direction], E.[Quantity]
UPDATE E
SET
	E.[MonetaryValue] = ROUND(AE.[AMVPU] * E.[BaseQuantity], C.E),
	E.[Value] = ROUND(AE.[AVPU] * E.[BaseQuantity], @E)
FROM map.DetailsEntries() E
JOIN AffectedEntries AE ON E.[LineId] = AE.[LineId] AND (E.[Index] = AE.[Index] OR E.[Index] = (AE.[Index] - 1))
JOIN dbo.Currencies C ON E.[CurrencyId] = C.[Id]