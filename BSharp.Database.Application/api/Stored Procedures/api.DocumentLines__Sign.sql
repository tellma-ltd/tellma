CREATE PROCEDURE [api].[DocumentLines__Sign]
-- TODO: Shall we modify it to api.DocumentLines__Sign ?
	@DocLinesIndexedIds dbo.[IndexedIdList] READONLY,
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
	
	DECLARE @DocLinesIds dbo.[IdList]
	INSERT INTO @DocLinesIds([Id]) SELECT [Id] FROM @DocLinesIndexedIds;

-- -- returns from the list of available agent roles the ones that are useful to move some docs @ToState
	DECLARE @DocLinesMissingSignatures dbo.[DocumentLineRoleList];
	INSERT INTO @DocLinesMissingSignatures([DocumentLineId], [RoleId])
	SELECT [DocumentLineId], [RoleId]
	FROM [rpt].[DocumentLines_ToState_Roles__Missing] (@DocLinesIds, @Roles, @ToState);

	-- Find additional role signatures required for documents satisfying workflow criteria
	DECLARE @DocLinesMissingConditionalSignatures dbo.[DocumentLineRoleList];
	INSERT INTO @DocLinesMissingConditionalSignatures([DocumentLineId], [RoleId])
	SELECT [DocumentLineId], [RoleId]
	FROM [rpt].[DocumentLines_ToState_Roles__MissingConditional] (@DocLinesIds, @Roles, @ToState);

	DECLARE	@DocLinesWithNoDefinedWorkflows dbo.[IdList];
	INSERT INTO @DocLinesWithNoDefinedWorkflows([Id])
	SELECT [DocumentLineId] FROM [rpt].[DocumentLinesWithNoDefinedWorkflows](@DocLinesIds);

	DECLARE @RelevantIndexedDocLines dbo.[IndexedIdList];
	INSERT INTO @RelevantIndexedDocLines([Index], [Id])
	SELECT [Index], [Id]
	FROM @DocLinesIndexedIds
	WHERE [Id] IN (
		SELECT [DocumentLineId] FROM @DocLinesMissingSignatures
		UNION
		SELECT [DocumentLineId]	FROM @DocLinesMissingConditionalSignatures
		UNION 
		SELECT [Id] FROM @DocLinesWithNoDefinedWorkflows
	);
	
	DECLARE @ValidationErrors dbo.[ValidationErrorList];
	INSERT INTO @ValidationErrors
	EXEC [bll].[DocumentLines_Validate__Sign]
		@Ids = @RelevantIndexedDocLines,
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

	DECLARE @RelevantDocLinesSignatures  dbo.[DocumentLineRoleList];
	INSERT INTO @RelevantDocLinesSignatures ([DocumentLineId], [RoleId])
	SELECT [DocumentLineId], [RoleId] FROM @DocLinesMissingSignatures
	UNION
	SELECT [DocumentLineId], [RoleId] FROM @DocLinesMissingConditionalSignatures

	EXEC [dal].[DocumentLines__Sign]
		@Entities = @RelevantDocLinesSignatures,
		@ToState = @ToState,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@AgentId = @AgentId,
		@SignedAt = @SignedAt;

	DECLARE @RelevantDocLinesIds dbo.IdList;
	INSERT INTO @RelevantDocLinesIds([Id]) SELECT [Id] FROM @RelevantIndexedDocLines;

	DECLARE @DocLinesConditionallyRequiringSignatures dbo.[DocumentLineRoleList];
	--SET @@DocLinesConditionallyRequiringSignatures ;-- TODO: must be filled by C#
	
	---- get the document Lines who satsified all the requirements for state change
	DECLARE @ReadyIds dbo.[IdList];
	INSERT INTO @ReadyIds([Id])
	EXEC [bll].[DocumentLines_Ready__Select]
		@DocumentLinesIds = @RelevantDocLinesIds,
		@ConditionalSignatures = @DocLinesConditionallyRequiringSignatures,
		@ToState = @ToState;
	
	EXEC [dal].[DocumentLines_State__Update]
		@Ids = @ReadyIds,
		@ToState = @ToState;

	DECLARE @FinalIds dbo.[IdList];
	INSERT INTO @FinalIds([Id])
	SELECT [Id] FROM @ReadyIds
	UNION
	SELECT [Id] FROM @DocLinesWithNoDefinedWorkflows;

	-- TODO: this code should be part of posting and voiding.
	EXEC [dal].[Documents__Assign]
		@Documents  = @FinalIds,
		@AssigneeId = NULL,
		@Comment	= NULL
END;