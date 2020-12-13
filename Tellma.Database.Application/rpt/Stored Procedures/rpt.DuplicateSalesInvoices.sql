CREATE PROCEDURE [rpt].[DuplicateSalesInvoices]
AS
	DECLARE @RevenueNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'Revenue');
	WITH RevenueAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Node].IsDescendantOf(@RevenueNode) = 1
		)
	)
	SELECT DISTINCT E.[ExternalReference] As Invoice, D.[Code]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
	JOIN RevenueAccounts A ON E.[AccountId] = A.[Id]
	JOIN
	(
		SELECT E.ExternalReference, E.[CustodyId] 
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
		JOIN RevenueAccounts A ON E.[AccountId] = A.[Id]
		WHERE D.[State] = 1
		AND E.ExternalReference like 'FS%'
		GROUP BY E.[ExternalReference], E.[CustodyId]
		HAVING COUNT(DISTINCT D.[Code]) > 1
	) T 
	ON E.[ExternalReference] = T.[ExternalReference]
	AND (E.[CustodyId] = T.[CustodyId] OR E.[CustodyId] IS NULL AND T.[CustodyId] IS NULL)
	WHERE D.[State] = 1
	ORDER BY E.[ExternalReference], D.[Code]