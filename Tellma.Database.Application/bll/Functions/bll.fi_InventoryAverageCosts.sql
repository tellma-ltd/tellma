CREATE FUNCTION [bll].[fi_InventoryAverageCosts] (
	@InventoryEntries dbo.[InventoryEntryList] READONLY
)
RETURNS TABLE AS RETURN (
	WITH InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.[AccountTypeId] = ATC.[Id]
		JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node])  = 1
		WHERE ATP.[Concept] = N'Inventories'
	)
-- Assumption 1: Resource cannot appear in more than one inventory account.
-- Assumption 2: Relation (Warehouse, Incoming Shippment, and Production Job) determines the business unit.
	SELECT
		IE.PostingDate, E.[RelationId] , E.[ResourceId],
		SUM(E.[Direction] * E.[BaseQuantity]) AS NetQuantity,
		SUM(E.[Direction] * E.[MonetaryValue]) AS NetMonetaryValue,
		SUM(E.[Direction] * E.[Value]) AS NetValue
	FROM map.[DetailsEntries]() E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN @InventoryEntries IE ON IE.[ResourceId] = E.[ResourceId] AND IE.[RelationId] = E.[RelationId] AND L.[PostingDate] <= IE.[PostingDate]
	JOIN InventoryAccounts A ON E.[AccountId] = A.[Id]
	WHERE L.[State] = 4
	GROUP BY IE.PostingDate, E.[RelationId] , E.[ResourceId]
);