CREATE PROCEDURE [api].[DocumentLines__Sign]
	@IndexedIds dbo.[IndexedIdList] READONLY,
	@ToState NVARCHAR(30),
	@ReasonId INT = NULL,
	@ReasonDetails	NVARCHAR(1024) = NULL,
	@AgentId INT = NULL, -- we allow selecting the agent manually, when entering from an external source document
	@RoleId INT = NULL, -- we allow selecting the role manually, 
	@SignedAt DATETIMEOFFSET(7) = NULL,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
/*
-- Only saved document lines can be signed
-- The documents lines might be in different states. The system knows for each line what are the missing roles signatures to move to next state
-- The system lists the missing roles signatures. This helps the user see if he can sign, and usign which role
-- The system shows all the states (positive and negative) accessible from the current state
-- The user selects the state he wants to move the lines to. By default, it is the min positive state
-- If the user is copying from another source document, the user can select the agent to sign on behalf of
-- The system shows all the roles of that agent
-- The user selects the role he wants to use for signing, by default it is the role with maximum lines
-- The system highlights the lines that can benefit from the user role signature
-- The user may de-select some of those lines (if trying to select other lines, the business logic will show it as an error)
-- The user signs with reason
-- The system refreshes the document lines and the list of accessible states.
-- The document can be filed/posted provided that:
	1) All the lines have reached their final states
	2) The lines with state Reviewed are balanced
-- The user may edit/save a document provided that the document is active
-- When a user modifies a line that was signed by others, there are two options:
	1) Flexible: System allows changing and alerts the prior users
	2) Rigid: System prevents changing. In this case, the prior users must unsign before editing
-- Accounts are affected by lines in state (Reviewed) where the document is (Posted)
*/
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];

	IF @RoleId NOT IN (SELECT RoleId FROM dbo.RoleMemberships WHERE AgentId = @AgentId)
	BEGIN
		
		RAISERROR(N'Error_IncompatibleAgentRole', 16, 1);
		RETURN
	END
	
	-- Validate that the agent is not violating any business logic attempting to move the relevant lines to State @ToState
	INSERT INTO @ValidationErrors
	EXEC [bll].[DocumentLines_Validate__Sign]
		@Ids = @IndexedIds,
		@AgentId = @AgentId,
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
	EXEC [dal].[DocumentLines__Sign]
		@Ids = @Ids,
		@ToState = @ToState,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@AgentId = @AgentId,
		@RoleId = @RoleId,
		@SignedAt = @SignedAt

	-- Determine which of the selected Lines are reacdy for state change
	DECLARE @ReadyIds dbo.IdList;
	INSERT INTO @ReadyIds SELECT [Id] FROM [bll].[fi_ReadyDocumentLines](@Ids, @ToState);

	EXEC dal.DocumentLines_State__Update @Ids = @ReadyIds, @ToState = @ToState;
END;