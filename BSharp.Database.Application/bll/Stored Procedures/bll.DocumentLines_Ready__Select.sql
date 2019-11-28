CREATE PROCEDURE [bll].[DocumentLines_Ready__Select]
	@DocumentLinesIds [dbo].[IdList] READONLY,
	@ConditionalSignatures [dbo].[DocumentLineRoleList] READONLY,
	@ToState NVARCHAR(30)
AS
WITH
RequiredSignatures AS (
	SELECT DL.[Id], WS.[RoleId]
	FROM dbo.[DocumentLines] DL
	JOIN dbo.[Workflows] W ON DL.[DefinitionId] = W.[LineDefinitionId]
	JOIN dbo.[WorkflowSignatures] WS ON W.[Id] = WS.[WorkflowId]
	WHERE DL.[Id] IN (SELECT [Id] FROM @DocumentLinesIds)
	AND W.[ToState] = @ToState
	AND WS.[Criteria] IS NULL
	--AND WS.[RevokedById] IS NULL
	--AND W.[RevokedById] IS NULL
	UNION
	SELECT [DocumentLineId], [RoleId]
	FROM @ConditionalSignatures
),
AvailableSignatures AS (
	SELECT DL.[Id], DS.[RoleId]
	FROM dbo.[DocumentLines] DL
	JOIN dbo.[DocumentLineSignatures] DS ON DL.[Id] = DS.[DocumentLineId]
	WHERE DL.[Id] IN (SELECT [Id] FROM @DocumentLinesIds)
	AND DS.[ToState] = @ToState
	AND DS.RevokedById IS NULL
	INTERSECT 
	SELECT * FROM RequiredSignatures
),
AvailableSignaturesCount AS (
	SELECT [Id], COUNT([RoleId]) AS [Count]
	FROM AvailableSignatures
	GROUP BY [Id]
),
RequiredSignaturedCount AS (
	SELECT [Id], COUNT([RoleId]) AS [Count]
	FROM RequiredSignatures
	GROUP BY [Id]
)
	SELECT A.[Id]
	FROM AvailableSignaturesCount A
	JOIN RequiredSignaturedCount R ON A.[Id] = R.[Id] AND A.[Count] = R.[Count]
	UNION 
	SELECT [Id]	FROM dbo.DocumentLines
	WHERE [Id] IN (SELECT [Id] FROM @DocumentLinesIds)
	AND [DefinitionId] NOT IN (
		SELECT [LineDefinitionId] FROM dbo.Workflows
	)


/*
	SELECT R.[Id]
	FROM
	(
		SELECT D.[Id], COUNT(WS.[RoleId]) AS SignaturesCount
		FROM dbo.[Documents] D
		JOIN dbo.Workflows W ON D.DocumentTypeId = W.DocumentTypeId
		JOIN dbo.WorkflowSignatories WS ON W.Id = WS.WorkflowId
		WHERE D.[Id] IN (SELECT [Id] FROM @Entities)
		AND W.ToState = @State
		AND WS.RevokedById IS NULL
		AND W.RevokedById IS NULL
		GROUP BY D.[Id]
	) R -- Required signature to move
	INNER JOIN
	(
		SELECT D.[Id], COUNT(DS.[RoleId]) AS SignaturesCount
		FROM dbo.[Documents] D
		JOIN dbo.DocumentSignatures DS ON D.Id = DS.DocumentId
		WHERE D.[Id] IN (SELECT [Id] FROM @Entities)
		AND DS.[State] = @State
		AND DS.RevokedById IS NULL
		GROUP BY D.[Id]
	) A -- Available signatures to move
	ON R.[Id] = A.[Id] AND R.SignaturesCount = A.SignaturesCount

*/