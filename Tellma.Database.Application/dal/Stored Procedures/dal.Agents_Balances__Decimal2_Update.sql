CREATE PROCEDURE [dal].[Agents_Balances__Decimal2_Update]
AS
-- Purchases and Sales invoices
WITH InvoicesBalances AS (
	SELECT E.[AgentId], AC.[Concept], SUM(E.[Direction] * E.[MonetaryValue]) AS Balance
	FROM dbo.Lines L
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
	JOIN dbo.AgentDefinitions AD ON AD.[Id] = AG.[DefinitionId]
	WHERE L.[State] = 4
	AND AC.[Concept] IN (N'TradeAndOtherCurrentPayablesToTradeSuppliers', N'CurrentTradeReceivables')
	AND AD.[Code] IN (N'PurchaseInvoice', N'SalesInvoice')
	GROUP BY E.[AgentId], AC.[Concept]
)
UPDATE AG
SET
	AG.[Decimal2] = ISNULL(B.[Balance], 0) * IIF(B.[Concept] = N'TradeAndOtherCurrentPayablesToTradeSuppliers', -1, 1)
--Select AG.*
FROM dbo.Agents AG
JOIN dbo.AgentDefinitions AD ON AD.[Id] = AG.[DefinitionId]
LEFT JOIN InvoicesBalances B ON B.[AgentId] = AG.[Id]
WHERE AD.[Code] IN (N'PurchaseInvoice', N'SalesInvoice')
AND ISNULL(AG.[Decimal2], -1) <> ISNULL(B.[Balance], 0) * IIF(B.[Concept] = N'TradeAndOtherCurrentPayablesToTradeSuppliers', -1, 1);

-- Supplier & Customer accounts
WITH AccountsBalances AS (
	SELECT AG.[Agent1Id] AS [AgentId], AD.[Code], SUM(AG.[Decimal2]) AS Balance
	FROM dbo.Agents AG
	JOIN dbo.AgentDefinitions AD ON AD.[Id] = AG.[DefinitionId]
	WHERE AD.[Code] IN (N'PurchaseInvoice', N'SalesInvoice')	
	AND AG.[Decimal2] IS NOT NULL
	GROUP BY AG.[Agent1Id], AD.[Code]
)
UPDATE AG
SET
	AG.[Decimal2] = ISNULL(B.[Balance], 0)
--Select AG.*
FROM dbo.Agents AG
JOIN dbo.AgentDefinitions AD ON AD.[Id] = AG.[DefinitionId]
LEFT JOIN AccountsBalances B ON B.[AgentId] = AG.[Id]
WHERE AD.[Code] IN (N'TradeReceivableAccount', N'TradePayableAccount')
AND ISNULL(AG.[Decimal2], 0) <> ISNULL(B.[Balance], 0);

-- Zero the rest
UPDATE AG
SET
	AG.[Decimal2] = 0
--Select AG.*
FROM dbo.Agents AG
JOIN dbo.AgentDefinitions AD ON AD.[Id] = AG.[DefinitionId]
WHERE AD.[Code] IN (N'TradeReceivableAccount', N'TradePayableAccount')
AND AG.[Decimal2] IS NULL;