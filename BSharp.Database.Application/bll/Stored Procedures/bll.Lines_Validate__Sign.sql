CREATE PROCEDURE [bll].[Lines_Validate__Sign]
	@Ids dbo.[IndexedIdList] READONLY,
	@AgentId INT,
	@RoleId INT,
	@ToState NVARCHAR(30),
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- TODO:
	-- Not allowed to cause negative fixed asset life
	-- Conservation of mass
	-- conservation of volume

	-- If signing on behalf of Agent
	IF (SELECT UserId FROM dbo.Agents WHERE [Id] = @AgentId) <> @UserId
	BEGIN
		-- If there is no proxy role, then Agent must sign in person
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_LineCannotBeSignedOnBehalOfAgent0',
			(SELECT [Name] FROM dbo.Agents WHERE [Id] = @AgentId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT DL.[Id] 
			FROM dbo.[Lines] DL
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = DL.[DefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState AND WS.[ProxyRoleId] IS NULL
		);

		-- if there is a proxy role, then User must have this role
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_User0LacksPermissionToSignLineOnBehalOfAgent1',
			(SELECT [Name] FROM dbo.Users WHERE [Id] = @UserId),
			(SELECT [Name] FROM dbo.Agents WHERE [Id] = @AgentId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT DL.[Id] 
			FROM dbo.[Lines] DL
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = DL.[DefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState
			AND WS.[ProxyRoleId] NOT IN (
				SELECT [RoleId] FROM dbo.RoleMemberships
				WHERE [UserId] = @UserId
			)
		);
	END
	-- verify that the line definition has a workflow transition from its current state to @ToState
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_NoDirectTransitionFromState0ToState1',
		DL.[State],
		@ToState
	FROM @Ids FE
	JOIN dbo.[Lines] DL ON FE.[Id] = DL.[Id]
	LEFT JOIN dbo.Workflows W ON W.[LineDefinitionId] = DL.[DefinitionId] AND W.[FromState] = DL.[State]
	WHERE W.ToState <> @ToState

	-- cannot sign lines unless the document is active. Document can be active, posted/filed	,
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_LineDoesNotBelongToActiveDocument'
	FROM @Ids FE
	JOIN dbo.[Lines] DL ON FE.[Id] = DL.[Id]
	JOIN dbo.Documents D ON DL.[DocumentId] = D.[Id]
	WHERE D.[State] <> 'Active'

	-- No inactive account
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheAccount0IsDeprecated',
		A.[Name]
	FROM @Ids FE
	JOIN dbo.[Entries] DLE ON FE.[Id] = DLE.[LineId]
	JOIN dbo.[Accounts] A ON A.[Id] = DLE.[AccountId]
	WHERE (A.[IsDeprecated] = 1);

	-- Not allowed to cause negative inventory balance
	WITH
	InventoryAccounts AS (
		SELECT [Id] FROM dbo.[Accounts] A
		WHERE A.[AccountTypeId] = N'Inventory'
	),
	CurrentDocLines AS (
		SELECT MAX(FE.[Index]) AS [Index], DLE.AccountId,
			SUM(DLE.[Direction] * DLE.[Mass]) AS [Mass], 
			SUM(DLE.[Direction] * DLE.[Volume]) AS [Volume], 
			SUM(DLE.[Direction] * DLE.[Count]) AS [Count]
		FROM @Ids FE
		JOIN dbo.[Entries] DLE ON FE.[Id] = DLE.[LineId]
		WHERE DLE.AccountId IN (SELECT [Id] FROM InventoryAccounts)
		GROUP BY DLE.AccountId
		HAVING SUM(DLE.[Direction] * DLE.[Mass]) < 0
		OR SUM(DLE.[Direction] * DLE.[Volume]) < 0
		OR SUM(DLE.[Direction] * DLE.[Count]) < 0
	),
	ReviewedDocLines AS (
		SELECT DLE.AccountId,
			SUM(DLE.[Direction] * DLE.[Mass]) AS [Mass], 
			SUM(DLE.[Direction] * DLE.[Volume]) AS [Volume], 
			SUM(DLE.[Direction] * DLE.[Count]) AS [Count]
		FROM dbo.DocumentLineEntriesDetailsView DLE
		JOIN CurrentDocLines C ON DLE.AccountId = C.AccountId 
		GROUP BY DLE.AccountId
	),
	OffendingEntries AS (
		SELECT C.[Index], C.AccountId, (C.[Mass] + P.[Mass]) AS [Mass]
		FROM CurrentDocLines C
		JOIN ReviewedDocLines P ON C.AccountId = P.AccountId
		WHERE (C.[Mass] + P.[Mass]) < 0
		OR (C.[Volume] + P.[Volume]) < 0
		OR (C.[Count] + P.[Count]) < 0
	)
	-- TODO: to be rewritten for each unit of measure. Also localize!
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST([Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheResource0Account1Shortage2',
		R.[Name], A.[Name], D.[Mass] -- 
	FROM OffendingEntries D
	JOIN dbo.[Accounts] A ON D.AccountId = A.Id
	JOIN dbo.Resources R ON A.ResourceId = R.Id

	SELECT TOP (@Top) * FROM @ValidationErrors;