CREATE PROCEDURE [api].[Documents__Sign]
	@DocsIndexedIds dbo.[IndexedIdList] READONLY,
	@ToState NVARCHAR(30),
	@ReasonId INT = NULL,
	@ReasonDetails	NVARCHAR(1024) = NULL,
	@AgentId INT = NULL,
	@SignedAt DATETIMEOFFSET(7) = NULL,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
-- TODO: Move the logic into bll functions and SProcs
SET NOCOUNT ON;
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	SET @SignedAt = ISNULL(@SignedAt, SYSDATETIMEOFFSET());
	SET @AgentId = ISNULL(@AgentId, @UserId);

	DECLARE @Roles dbo.[IdList];
	INSERT INTO @Roles([Id])
	SELECT [RoleId]	FROM dbo.RoleMemberships WHERE AgentId = @AgentId;
	
	DECLARE @DocsIds dbo.[IdList]
	INSERT INTO @DocsIds([Id]) SELECT [Id] FROM @DocsIndexedIds;

-- -- returns from the list of available agent roles the ones that are useful to move some docs @ToState
	DECLARE @DocsMissingSignatures dbo.[DocumentRoleList];
	INSERT INTO @DocsMissingSignatures([DocumentId], [RoleId])
	SELECT [DocumentId], [RoleId]
	FROM [rpt].[Documents_ToState_Roles__Missing] (@DocsIds, @Roles, @ToState);

	-- Find additional role signatures required for documents satisfying workflow criteria
	DECLARE @DocsMissingConditionalSignatures dbo.[DocumentRoleList];
	INSERT INTO @DocsMissingConditionalSignatures([DocumentId], [RoleId])
	SELECT [DocumentId], [RoleId]
	FROM [rpt].[Documents_ToState_Roles__MissingConditional] (@DocsIds, @Roles, @ToState);

	DECLARE	@DocsWithNoDefinedWorkflows dbo.[IdList];
	INSERT INTO @DocsWithNoDefinedWorkflows([Id])
	SELECT [Id] FROM [rpt].[DocumentsWithNoDefinedWorkflows](@DocsIds);

	DECLARE @RelevantIndexedDocs dbo.[IndexedIdList];
	INSERT INTO @RelevantIndexedDocs([Index], [Id])
	SELECT [Index], [Id]
	FROM @DocsIndexedIds
	WHERE [Id] IN (
		SELECT [DocumentId] FROM @DocsMissingSignatures
		UNION
		SELECT [DocumentId]	FROM @DocsMissingConditionalSignatures
		UNION 
		SELECT [Id] FROM @DocsWithNoDefinedWorkflows
	);
	
	DECLARE @ValidationErrors dbo.[ValidationErrorList];
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Sign]
		@Ids = @RelevantIndexedDocs,
		@AgentId = @AgentId,
		@ToState = @ToState;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	DECLARE @RelevantDocsSignatures  dbo.[DocumentRoleList];
	INSERT INTO @RelevantDocsSignatures ([DocumentId], [RoleId])
	SELECT [DocumentId], [RoleId] FROM @DocsMissingSignatures
	UNION
	SELECT [DocumentId], [RoleId] FROM @DocsMissingConditionalSignatures

	EXEC [dal].[Documents__Sign]
		@Entities = @RelevantDocsSignatures,
		@ToState = @ToState,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@AgentId = @AgentId,
		@SignedAt = @SignedAt;

	DECLARE @RelevantDocsIds dbo.IdList;
	INSERT INTO @RelevantDocsIds([Id]) SELECT [Id] FROM @RelevantIndexedDocs;

	DECLARE @DocsConditionallyRequiringSignatures dbo.[DocumentRoleList];
	--SET @DocsConditionallyRequiringSignatures ;-- TODO: must be filled by C#
	
	---- get the documents who satsified all the requirements for state change
	DECLARE @ReadyIds dbo.[IdList];
	INSERT INTO @ReadyIds([Id])
	EXEC [bll].[Documents_Ready__Select]
		@Ids = @RelevantDocsIds,
		@ConditionalSignatures = @DocsConditionallyRequiringSignatures,
		@ToState = @ToState;
	
	EXEC [dal].[Documents_State__Update]
		@Ids = @ReadyIds,
		@ToState = @ToState;

	DECLARE @FinalIds dbo.[IdList];
	INSERT INTO @FinalIds([Id])
	SELECT [Id] FROM @ReadyIds
	UNION
	SELECT [Id] FROM @DocsWithNoDefinedWorkflows;

	EXEC [dal].[Documents__Assign]
		@Documents  = @FinalIds,
		@AssigneeId = NULL,
		@Comment	= NULL
END;