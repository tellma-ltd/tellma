CREATE PROCEDURE [dal].[InventoriesInTransit_Credit__Update]
@CustodyId INT
AS
-- For each LC center (also duplicated as custody)
-- For each resource
--	Sum all the Quantities debited to IIT account (increase/decrease through other changes)
--	Sum all the values debited to IIT account (either directly or by re-assignment from Expenses)
--	Compute Sum of Value / Sum of Quantity
-- For each LD involving IIT
--	Update the credit entry with the new average price
--	Update the related debit entry as well. 
SELECT SUM(E.[Quantity]) AS TotalQuantity, SUM(MonetaryValue) AS TotalMonetaryValue, SUM([Value]) AS TotalValue
FROM map.DetailsEntries() E
JOIN dbo.Lines L ON L.[Id] = E.[LineId]
WHERE L.[State] = 4
AND (@CustodyId IS NULL OR [CustodyId] = @CustodyId)