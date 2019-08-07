CREATE PROCEDURE [api].[Documents__Sign]
	@Entities [dbo].[IdList] READONLY,
	@State NVARCHAR(30),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@AgentId INT,
	@RoleId INT,
	@ReturnEntities BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
DECLARE @FilteredEntities [dbo].[IdList], @ReadyEntities [dbo].[IdList];

	INSERT INTO @FilteredEntities([Id])
	EXEC [bll].[Documents_Filter__Sign]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Documents__Sign]
		@Entities = @FilteredEntities,
		@State = @State,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@AgentId = @AgentId,
		@RoleId = @RoleId

	-- get the documents who satsified all the requirements for state change
	

	INSERT INTO @ReadyEntities([Id])
	EXEC [bll].[Documents_Ready__Select]
	
	IF EXISTS(SELECT * FROM @ReadyEntities)
		EXEC [dal].[Documents_State__Update]
			@Entities = @ReadyEntities,
			@State = @State;

	IF @ReturnEntities = 1
		SELECT * FROM @ReadyEntities;
END;