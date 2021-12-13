CREATE PROCEDURE [bll].[Lines_Ready__Select]
	@LinesIds [dbo].[IdList] READONLY,
	@ConditionalSignatures [dbo].[LineRoleList] READONLY,
	@ToState SMALLINT -- NVARCHAR(30)
AS
WITH
RequiredSignatures AS (
	SELECT DL.[Id], WS.[RoleId]
	FROM dbo.[Lines] DL
	JOIN dbo.[Workflows] W ON DL.[DefinitionId] = W.[LineDefinitionId]
	JOIN dbo.[WorkflowSignatures] WS ON W.[Id] = WS.[WorkflowId]
	WHERE DL.[Id] IN (SELECT [Id] FROM @LinesIds)
	AND W.[ToState] = @ToState
	AND WS.[PredicateType] IS NULL
	--AND WS.[RevokedById] IS NULL
	--AND W.[RevokedById] IS NULL
	UNION
	SELECT [LineId], [RoleId]
	FROM @ConditionalSignatures
),
AvailableSignatures AS (
	SELECT DL.[Id], DS.[RoleId]
	FROM dbo.[Lines] DL
	JOIN dbo.[LineSignatures] DS ON DL.[Id] = DS.[LineId]
	WHERE DL.[Id] IN (SELECT [Id] FROM @LinesIds)
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
	SELECT [Id]	FROM dbo.[Lines]
	WHERE [Id] IN (SELECT [Id] FROM @LinesIds)
	AND [DefinitionId] NOT IN (
		SELECT [LineDefinitionId] FROM dbo.Workflows
	)