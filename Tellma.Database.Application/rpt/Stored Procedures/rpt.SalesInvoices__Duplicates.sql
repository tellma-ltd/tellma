CREATE PROCEDURE [rpt].[SalesInvoices__Duplicates]
AS
	DECLARE @RevenueNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'Revenue');
	WITH RevenueAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Node].IsDescendantOf(@RevenueNode) = 1
		)
	)
	SELECT DISTINCT E.[InternalReference] As Invoice, D.[Code]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
	JOIN RevenueAccounts A ON E.[AccountId] = A.[Id]
	JOIN
	(
		SELECT E.[InternalReference], E.[CustodyId] 
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
		JOIN RevenueAccounts A ON E.[AccountId] = A.[Id]
		WHERE D.[State] = 1
		AND E.[InternalReference] like 'FS%'
		GROUP BY E.[InternalReference], E.[CustodyId]
		HAVING COUNT(DISTINCT D.[Code]) > 1
	) T 
	ON E.[InternalReference] = T.[InternalReference]
	AND (E.[CustodyId] = T.[CustodyId] OR E.[CustodyId] IS NULL AND T.[CustodyId] IS NULL)
	WHERE D.[State] = 1
	ORDER BY E.[InternalReference], D.[Code]

	-- 
	SELECT E.[InternalReference], MIN(D.[Code]) AS Doc1, MAX(D.[Code]) AS Doc2
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
	JOIN dbo.LineDefinitions LD ON L.[DefinitionId] = LD.[Id]
	WHERE D.[State] = 1
	AND LD.Code = N'CustomerPeriodInvoice'
	AND E.[InternalReference] like 'FS%'
	GROUP BY E.[InternalReference]--, E.[CustodyId]
	HAVING COUNT(DISTINCT D.[Code]) > 1