CREATE PROCEDURE [rpt].[DuplicatePurchasesInvoices]
AS
	DECLARE @ExpenseByNatureNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'ExpenseByNature');
	WITH ExpenseByNatureAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Node].IsDescendantOf(@ExpenseByNatureNode) = 1
		)
	)
	SELECT DISTINCT E.[ExternalReference] As Invoice, D.[Code]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
	JOIN ExpenseByNatureAccounts A ON E.[AccountId] = A.[Id]
	JOIN
	(
		SELECT E.ExternalReference, E.[NotedRelationId] 
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
		JOIN ExpenseByNatureAccounts A ON E.[AccountId] = A.[Id]
		WHERE D.[State] = 1
		AND E.[NotedRelationId] is NOT NULL
		AND E.ExternalReference like 'FS%'
		GROUP BY E.[ExternalReference], E.[NotedRelationId]
		HAVING COUNT(DISTINCT D.[Code]) > 1
	) T 
	ON E.[ExternalReference] = T.[ExternalReference]
	AND (E.[NotedRelationId] = T.[NotedRelationId])
	WHERE D.[State] = 1
	ORDER BY E.[ExternalReference], D.[Code]