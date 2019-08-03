CREATE PROCEDURE [dbo].[api_Documents__Sign]
	@Entities [dbo].[UuidList] READONLY,
	@State NVARCHAR(255),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@AgentId INT,
	@RoleId INT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	EXEC [dbo].[bll_Documents_Validate__Sign]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_Documents__Sign]
		@Entities = @Entities,
		@State = @State,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@AgentId = @AgentId,
		@RoleId = @RoleId

	-- get the documents whose state will change
	DECLARE @TransitionedIds [dbo].[UiidWithStateList];
	/*
	INSERT INTO @TransitionedIds([Id])
	EXEC [dbo].[bll_Documents_State__Select]
	*/
	IF EXISTS(SELECT * FROM @TransitionedIds)
		EXEC dal_Documents_State__Update @Entities = @TransitionedIds
END;