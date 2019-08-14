CREATE PROCEDURE [bll].[Documents_Ready__Select]
	@Ids [dbo].[IdList] READONLY,
	@ConditionalSignatures [dbo].[DocumentRoleList] READONLY,
	@ToState NVARCHAR(30)
AS
WITH
RequiredSignatures AS (
	SELECT D.[Id], WS.[RoleId]
	FROM dbo.[Documents] D
	JOIN dbo.[Workflows] W ON D.[DocumentTypeId] = W.[DocumentTypeId]
	JOIN dbo.[WorkflowSignatures] WS ON W.[Id] = WS.[WorkflowId]
	WHERE D.[Id] IN (SELECT [Id] FROM @Ids)
	AND W.[ToState] = @ToState
	AND WS.[Criteria] IS NULL
	--AND WS.[RevokedById] IS NULL
	--AND W.[RevokedById] IS NULL
	UNION
	SELECT [DocumentId], [RoleId]
	FROM @ConditionalSignatures
),
AvailableSignatures AS (
	SELECT D.[Id], DS.[RoleId]
	FROM dbo.[Documents] D
	JOIN dbo.[DocumentSignatures] DS ON D.[Id] = DS.[DocumentId]
	WHERE D.[Id] IN (SELECT [Id] FROM @Ids)
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
	SELECT [Id]	FROM dbo.Documents
	WHERE [Id] IN (SELECT [Id] FROM @Ids)
	AND [DocumentTypeId] NOT IN (
		SELECT [DocumentTypeId] FROM dbo.Workflows
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