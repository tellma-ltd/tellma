CREATE PROCEDURE [api].[Lines__Sign]
	@IndexedIds dbo.[IndexedIdList] READONLY,
	@ToState SMALLINT,
	@ReasonId INT = NULL,
	@ReasonDetails	NVARCHAR(1024) = NULL,
	@OnBehalfOfUserId INT = NULL,
	@RuleType NVARCHAR (50),
	@RoleId INT = NULL,
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
-- In certain cases, the user can select the user to sign on behalf of
-- The user selects the lines he wants to sign
-- The system shows all the required signatures, and whether the current user can sign them, or on behalf
-- The user selects the signatures he wants to invoke. for each line, it is the signature corresponding to the minimum state (in absolute)
that has not been signed
-- The system refreshes the document lines and the list of accessible states.
-- The document can be closed provided that:
	1) All the lines have reached their final states
	2) The lines with state FINALIZED are balanced
-- The user may edit/save a document provided that the document is active
-- When a user modifies a line that was signed by others, there are two options:
	1) Flexible: System allows changing and alerts the prior users
	2) Rigid: System prevents changing. In this case, the prior users must unsign before editing
-- Accounts are affected by lines in state (REVIEWED) where the document is (Posted)
*/
SET NOCOUNT ON;
	DECLARE @Ids [dbo].[IdList];

	-- Validate that the user is not violating any business logic attempting to move the relevant lines to State @ToState

	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__Sign]
		@Ids = @IndexedIds,
		@OnBehalfOfUserId = @OnBehalfOfUserId,
		@RuleType = @RuleType,
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
	EXEC [dal].[Lines__SignAndRefresh]
		@Ids = @Ids,
		@ToState = @ToState,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@OnBehalfOfUserId = @OnBehalfOfUserId,
		@RuleType = @RuleType,
		@RoleId = @RoleId,
		@SignedAt = @SignedAt
END;