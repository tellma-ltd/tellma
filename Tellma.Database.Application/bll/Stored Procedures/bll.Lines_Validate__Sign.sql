CREATE PROCEDURE [bll].[Lines_Validate__Sign]
	@Ids dbo.[IndexedIdList] READONLY,
	@OnBehalfOfuserId INT,
	@RuleType NVARCHAR (50),
	@RoleId INT = NULL,
	@ToState SMALLINT,
	@Top INT = 10
	--'ToState', 'RuleType', 'RoleId', 'AgentId', 'UserId', 'SignedById', 'SignedAt', 'OnBehalfOfUserId',
   -- 'LastUnsignedState', 'LastNegativeState', 'CanSign', 'ProxyRoleId', 'CanSignOnBehalf',
    --'ReasonId', 'ReasonDetails'
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Verify that the signing UserId fulfills one of the required signature
	-- Corollary: Signatures are not repeated if signing twice in a row 
	-- TODO:
	-- No inactive Resource, No inactive User

	IF @OnBehalfOfuserId IS NULL SET @OnBehalfOfuserId = @UserId
	-- Must not sign a document that is already posted/canceled
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		CASE
			WHEN D.[PostingState] = 1 THEN N'Error_CannotSignPostedDocuments'
			WHEN D.[PostingState] = -1 THEN N'Error_CannotSignCanceledDocuments'
		END
	FROM @Ids FE
	JOIN dbo.Lines L ON FE.[Id] = L.[Id]
	JOIN [dbo].[Documents] D ON D.[Id] = L.[DocumentId]
	WHERE D.[PostingState] <> 0; -- Posted or Canceled

	IF @RuleType = N'ByRole'
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
	-- TODO: Cannot sign a line where CanSign = 0
	--INSERT INTO @ValidationErrors([Key], [ErrorName])		
	--SELECT DISTINCT TOP (@Top)
	--	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
	--	N'Error_UserCannotSignLine'
	--FROM map.[LinesRequiredSignatures](@LineIds) RS
	--JOIN @Ids FE ON RS.LineId = FE.Id
	--WHERE RS.CanSign = 0;

	-- Cannot sign a current state, unless all states < abs (current state) are positive and signed.	
	INSERT INTO @ValidationErrors([Key], [ErrorName])		
	SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			CASE 
				WHEN LastUnsignedState = 1 THEN N'Error_Line0MustBeRequestedFirst'
				WHEN LastUnsignedState = 2 THEN N'Error_Line0MustBeAuthorizedFirst'
				WHEN LastUnsignedState = 3 THEN N'Error_Line0MustBeCompletedFirst'
			END
	FROM map.[LinesRequiredSignatures](@LineIds) RS
	JOIN @Ids FE ON RS.LineId = FE.Id
	WHERE ToState = ABS(@ToState) AND LastUnsignedState IS NOT NULL

	-- Cannot sign a current state, if it is already signed negatively in a previous state.
	INSERT INTO @ValidationErrors([Key], [ErrorName])		
	SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			CASE
				WHEN LastNegativeState = -1 THEN N'Error_LineIsAlready_minus_1'
				WHEN LastNegativeState = -2 THEN N'Error_LineIsAlready_minus_2'
				WHEN LastNegativeState = -3 THEN N'Error_LineIsAlready_minus_3'
				WHEN LastNegativeState = -4 THEN N'Error_LineIsAlready_minus_4'
			END
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

	DECLARE @Lines LineList, @Entries EntryList;
	INSERT INTO @Lines([Index], [DocumentIndex], [Id], [DefinitionId], [Memo])
	SELECT L.[Index], L.[DocumentId], L.[Id], L.[DefinitionId], L.[Memo]
	FROM dbo.Lines L
	WHERE L.[Id] IN (SELECT [ID] FROM @Ids) 
	INSERT INTO @Entries ([Index],[LineIndex],[DocumentIndex],[Id],
	[Direction],[AccountId],[CurrencyId],[AgentId],[ResourceId],[CenterId],
	[EntryTypeId],[DueDate],[MonetaryValue],[Quantity],[UnitId],[Value],[Time1],
	[Time2]	,[ExternalReference],[AdditionalReference],[NotedAgentId],[NotedAgentName],
	[NotedAmount],[NotedDate])
	SELECT E.[Index],L.[Index],L.[DocumentId],E.[Id],
	E.[Direction],E.[AccountId],E.[CurrencyId],E.[AgentId],E.[ResourceId],E.[CenterId],
	E.[EntryTypeId],E.[DueDate],E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value],E.[Time1],
	E.[Time2]	,E.[ExternalReference],E.[AdditionalReference],E.[NotedAgentId],E.[NotedAgentName],
	E.[NotedAmount],E.[NotedDate]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id];
	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__State_Update]
	@Lines = @Lines, @Entries = @Entries, @ToState = @ToState;
	SELECT TOP (@Top) * FROM @ValidationErrors;