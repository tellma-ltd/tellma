CREATE PROCEDURE [bll].[Lines_Validate__Sign]
	@Ids dbo.[IndexedIdList] READONLY,
	@OnBehalfOfuserId INT,
	@RuleType NVARCHAR (50),
	@RoleId INT = NULL,
	@ToState SMALLINT,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Verify that the signing UserId fulfills one of the required signature
	-- Corollary: Signatures are not repeated if signing twice in a row 
	-- TODO:
	-- Not allowed to cause negative fixed asset life among states >= completed, if neg is not allowed.
	-- Conservation of mass
	-- conservation of volume
	-- No inactive Resource, No inactive User

	IF @OnBehalfOfuserId IS NULL SET @OnBehalfOfuserId = @UserId

	-- Must not sign a document that is already posted
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		CASE
			WHEN D.[PostingState] = 1 THEN N'Error_CannotSignPostedDocuments'
			WHEN D.[PostingState] = -1 THEN N'Error_CannotSignCanceledDocuments'
		END
	FROM @Ids FE
	JOIN dbo.Lines L ON FE.[Id] = L.[Id]
	JOIN [dbo].[Documents] D ON D.[Id] = L.[DocumentId]
	WHERE D.[PostingState] <> 0; -- Posted or Canceled

	IF @RoleId NOT IN (
		SELECT RoleId FROM dbo.RoleMemberships 
		WHERE [UserId] = @OnBehalfOfuserId
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	VALUES (
		N'UserId',
		N'Error_IncompatibleUser0Role1',
		(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Users WHERE [Id] = @OnBehalfOfuserId),
		(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Roles WHERE [Id] = @RoleId)
	);

	DECLARE @LineIds IdList;
	INSERT INTO @LineIds([Id]) SELECT [Id] FROM @Ids;

	-- Cannot sign a current state, unless all states < abs (current state) are positive and signed.	
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])		
	SELECT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_Line0MustBeSignedForState1BeforeState2',
			FE.Id AS LineId,
			[dbo].[fn_StateId__State](LastUnsignedState),
			[dbo].[fn_StateId__State](@ToState)
	FROM map.[LinesRequiredSignatures](@LineIds) RS
	JOIN @Ids FE ON RS.LineId = FE.Id
	WHERE ToState = ABS(@ToState) AND LastUnsignedState IS NOT NULL

	-- Cannot sign a current state, if it is already signed negatively in a previous state.
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])		
	SELECT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_Line0IsAlreadyInState1',
			FE.Id AS LineId,
			[dbo].[fn_StateId__State](LastNegativeState)
	FROM map.[LinesRequiredSignatures](@LineIds) RS
	JOIN @Ids FE ON RS.LineId = FE.Id
	WHERE LastNegativeState IS NOT NULL

	-- If signing on behalf of User
	IF (@OnBehalfOfuserId IS NOT NULL) AND (@OnBehalfOfuserId <> @UserId)
	BEGIN
		-- If there is no proxy role, then User must sign in person
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_LineCannotBeSignedOnBehalOfUser0',
			(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Users WHERE [Id] = @OnBehalfOfuserId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT L.[Id] 
			FROM dbo.[Lines] L
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = L.[DefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState AND WS.RuleType = @RuleType AND WS.[ProxyRoleId] IS NULL
		);

		-- if there is a proxy role, then User must have this role
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_User0LacksPermissionToSignLineOnBehalfOfUser1',
			(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Users WHERE [Id] = @UserId),
			(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Users WHERE [Id] = @OnBehalfOfuserId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT L.[Id] 
			FROM dbo.[Lines] L
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = L.[DefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState
			AND WS.RuleType = @RuleType
			AND WS.[ProxyRoleId] NOT IN (
				SELECT [RoleId] FROM dbo.RoleMemberships
				WHERE [UserId] = @UserId
			)
		);
	END

	-- cannot sign a line by Agent, if Agent/UserId is null
	IF @RuleType = N'ByAgent'
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgent0HasNoUserId',
		dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]) AS AgentName
	FROM @Ids FE
	JOIN dbo.Lines L ON FE.[Id] = L.[Id]
	JOIN dbo.Entries E ON L.[Id] = E.[LineId]
	JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
	JOIN dbo.Workflows W ON W.LineDefinitionId = L.DefinitionId AND W.ToState = @ToState
	JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
	WHERE WS.RuleType = N'ByAgent' AND WS.[RuleTypeEntryIndex]  = E.[Index]
	AND AG.UserId IS NULL

	-- Cannot sign a line with no Entries
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheLineHasNoEntries'
	FROM @Ids FE
	LEFT JOIN dbo.Entries E ON FE.[Id] = E.[LineId]
	WHERE E.[Id] IS NULL;

	-- No Null account when moving to state 4
	IF @ToState = 4 -- reviewed
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0],[Argument1],[Argument2],[Argument3],[Argument4])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index]  AS NVARCHAR (255))+ '].AccountId',
		N'Error_LineNoAccountForEntryIndex0WithAccountType1Currency2Agent3Resource4',
		E.[Index],
		LDE.[AccountTypeParentCode],
		E.[CurrencyId],
		dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]),
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3])
	FROM @Ids FE
	JOIN dbo.Entries E ON FE.[Id] = E.LineId
	JOIN dbo.Lines L ON L.[Id] = FE.[Id]
	LEFT JOIN dbo.LineDefinitionEntries LDE ON LDE.LineDefinitionId = L.DefinitionId AND LDE.[Index] = E.[Index]
	LEFT JOIN dbo.Agents AG ON E.AgentId = AG.Id
	LEFT JOIN dbo.Resources R ON E.ResourceId = R.Id
	WHERE E.AccountId IS NULL

	-- No deprecated account, for any positive state
	IF @ToState > 0
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheAccount0IsDeprecated',
		A.[Name]
	FROM @Ids FE
	JOIN dbo.[Entries] E ON FE.[Id] = E.[LineId]
	JOIN dbo.[Accounts] A ON A.[Id] = E.[AccountId]
	WHERE (A.[IsDeprecated] = 1);

	-- Not allowed to cause negative balance in conservative accounts
	--DECLARE @NonFinancialResourceClassificationNode HIERARCHYID = 
	--	(SELECT [Node] FROM dbo.[AccountTypes] WHERE Code = N'NonFinancialAssets');
	--WITH
	--ConservativeAccounts AS (
	--	SELECT [Id] FROM dbo.[Accounts] A
	--	--TODO: use  Account Type instead, and limit to state COMPLETED
	--	WHERE A.[LegacyTypeId] = N'OnHand'
	--	AND A.[AccountTypeId] IN (
	--		SELECT [Id] FROM dbo.[AccountTypes]
	--		WHERE [Node].IsDescendantOf(@NonFinancialResourceClassificationNode) = 1
	--	)
	--),
	--CurrentDocLines AS (
	--	SELECT MAX(FE.[Index]) AS [Index],
	--		E.AccountId,
	--		E.ResourceId,
	--		E.AgentId,
	--		E.DueDate,
	--		--E.[AccountIdentifier],
	--		--E.[ResourceIdentifier],
	--		SUM(E.[Direction] * E.[Count]) AS [Count],
	--		SUM(E.[Direction] * E.[Mass]) AS [Mass], 
	--		SUM(E.[Direction] * E.[Volume]) AS [Volume] 
			
	--	FROM @Ids FE
	--	-- TODO: change to map.DetailsEntries
	--	JOIN dbo.[Entries] E ON FE.[Id] = E.[LineId]
	--	WHERE E.AccountId IN (SELECT [Id] FROM ConservativeAccounts)
	--	GROUP BY
	--		E.AccountId,
	--		E.ResourceId,
	--		E.AgentId,
	--		E.DueDate--,
	--		--E.[AccountIdentifier],
	--		--E.[ResourceIdentifier]
	--	HAVING
	--		SUM(E.[Direction] * E.[Mass]) < 0
	--	OR	SUM(E.[Direction] * E.[Volume]) < 0
	--	OR	SUM(E.[Direction] * E.[Count]) < 0
	--),
	--ReviewedDocLines AS (
	--	SELECT
	--		E.AccountId,
	--		E.ResourceId,
	--		E.AgentId,
	--		E.DueDate,
	--		--E.[AccountIdentifier],
	--		--E.[ResourceIdentifier],
	--		SUM(E.[Direction] * E.[Mass]) AS [Mass], 
	--		SUM(E.[Direction] * E.[Volume]) AS [Volume], 
	--		SUM(E.[Direction] * E.[Count]) AS [Count]
	--	FROM dbo.Entries E JOIN dbo.Lines L ON L.[Id] = E.[LineId] JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	--	JOIN CurrentDocLines C ON E.AccountId = C.AccountId 
	--	GROUP BY
	--		E.AccountId,
	--		E.ResourceId,
	--		E.AgentId,
	--		E.[DueDate]
	--		--E.[AccountIdentifier],
	--		--E.[ResourceIdentifier]
	--),
	--OffendingEntries AS (
	--	SELECT C.[Index], C.AccountId, (C.[Mass] + P.[Mass]) AS [Mass]
	--	FROM CurrentDocLines C
	--	JOIN ReviewedDocLines P ON
	--		C.AccountId = P.AccountId
	--	AND (C.ResourceId = P.ResourceId)
	--	AND (C.AgentId = P.AgentId)
	--	AND (C.[DueDate] = P.[DueDate] OR (C.[DueDate] IS NULL AND P.[DueDate] IS NULL))
	--	--AND (C.[AccountIdentifier] = P.[AccountIdentifier] OR (C.[AccountIdentifier] IS NULL AND P.[AccountIdentifier] IS NULL))
	--	--AND (C.[ResourceIdentifier] = P.[ResourceIdentifier] OR (C.[ResourceIdentifier] IS NULL AND P.[ResourceIdentifier] IS NULL))
	--	WHERE
	--		(C.[Count] + P.[Count]) < 0
	--	OR	(C.[Mass] + P.[Mass]) < 0
	--	OR	(C.[Volume] + P.[Volume]) < 0
	--)
	---- TODO: to be rewritten for each unit of measure.
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	--SELECT TOP (@Top)
	--	'[' + ISNULL(CAST([Index] AS NVARCHAR (255)),'') + ']', 
	--	N'Error_TheResource0Account1Shortage2',
	--	dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource], 
	--	dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) AS [Account],
	--	D.[Mass] -- 
	--FROM OffendingEntries D
	--JOIN dbo.[Accounts] A ON D.AccountId = A.Id
	--JOIN dbo.Resources R ON A.ResourceId = R.Id

	SELECT TOP (@Top) * FROM @ValidationErrors;