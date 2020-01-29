CREATE PROCEDURE [api].[Lines__Sign]
	@IndexedIds dbo.[IndexedIdList] READONLY,
	@ToState SMALLINT,
	@ReasonId INT = NULL,
	@ReasonDetails	NVARCHAR(1024) = NULL,
	@OnBehalfOfuserId INT = NULL, -- we allow selecting the user manually, when entering from an external source document
	@RoleId INT = NULL, -- we allow selecting the role manually, 
	@SignedAt DATETIMEOFFSET(7) = NULL,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
/*
-- Only saved document lines can be signed
-- The documents lines might be in different states. The system knows for each line what are the missing roles signatures to move to next state
-- A line can be split and merged, and remains in the same state with the same signatures
-- A line can be moved to a separate document of the same type, and remains in the same state and same signatures
-- The system lists the missing roles signatures. This helps the user see if he can sign, and usign which role
-- The system shows all the states (positive and negative) accessible from the current state
-- The user selects the state he wants to move the lines to. By default, it is the min positive state
-- If the user is copying from another source document, the user can select the user to sign on behalf of
-- The system shows all the roles of that user
-- The user selects the role he wants to use for signing, by default it is the role with maximum lines
-- The system highlights the lines that can benefit from the user role signature
-- The user may de-select some of those lines (if trying to select other lines, the business logic will show it as an error)
-- The user signs with reason
-- The system refreshes the document lines and the list of accessible states.
-- The document can be closed provided that:
	1) All the lines have reached their final states
	2) The lines with state REVIEWED are balanced
-- The user may edit/save a document provided that the document is active
-- When a user modifies a line that was signed by others, there are two options:
	1) Flexible: System allows changing and alerts the prior users
	2) Rigid: System prevents changing. In this case, the prior users must unsign before editing
-- Accounts are affected by lines in state (REVIEWED) where the document is (Posted)
*/
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];

	IF @RoleId NOT IN (
		SELECT RoleId FROM dbo.RoleMemberships 
		WHERE [UserId] = @OnBehalfOfuserId
	)
	BEGIN
		RAISERROR(N'Error_IncompatibleUserRole', 16, 1);
		RETURN
	END
	
	-- Validate that the user is not violating any business logic attempting to move the relevant lines to State @ToState
	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__Sign]
		@Ids = @IndexedIds,
		@OnBehalfOfuserId = @OnBehalfOfuserId,
		@RoleId = @RoleId,
		@ToState = @ToState;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[Lines__Sign]
		@Ids = @Ids,
		@ToState = @ToState,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@OnBehalfOfuserId = @OnBehalfOfuserId,
		@RoleId = @RoleId,
		@SignedAt = @SignedAt

	-- Determine which of the selected Lines are reacdy for state change
	DECLARE @ReadyIds dbo.IdList, @LinesSatisfyingCriteria IdWithCriteriaList;
		/*
	-- C#: In C#, the system shall verify which lines satsify the corresponding criteria and return the result in @LinesCriteria
		SELECT L.[Id], WS.[Criteria]
		FROM dbo.Lines L
		JOIN dbo.Workflows W ON L.[DefinitionId] = W.[LineDefinitionId]
		JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId] AND L.[State] = W.[FromState]
		WHERE W.[ToState] = @ToState AND WS.[Criteria] IS NOT NULL
		AND L.[Id] IN (
			SELECT [Id] FROM @@IndexedIds
		)
	*/
	INSERT INTO @ReadyIds SELECT [Id] FROM [bll].[fi_Lines__Ready](@Ids, @ToState, @LinesSatisfyingCriteria);

	EXEC dal.[Lines_State__Update] @Ids = @ReadyIds, @ToState = @ToState;

	DECLARE @DocIds dbo.IdList;
	INSERT INTO @DocIds([Id])
	SELECT DISTINCT DocumentId FROM dbo.Lines
	WHERE [Id] IN (SELECT [Id] FROM @IndexedIds);

	EXEC dal.Documents_State__Refresh @DocIds;
END;