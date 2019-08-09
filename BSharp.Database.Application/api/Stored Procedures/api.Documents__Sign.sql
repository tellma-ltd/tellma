CREATE PROCEDURE [api].[Documents__Sign]
	@Entities [dbo].[IdList] READONLY,
	@State NVARCHAR(30),
	@ReasonId INT = NULL,
	@ReasonDetails	NVARCHAR(1024) = NULL,
	@AgentId INT = NULL,
	@RoleId INT = NULL,
	@SignedAt DATETIMEOFFSET(7) = NULL,
	@ReturnEntities BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
DECLARE @FilteredEntities [dbo].[IdList], @ReadyEntities [dbo].[IdList];
SET @SignedAt = ISNULL(@SignedAt, SYSDATETIMEOFFSET());
SET @AgentId = ISNULL(@AgentId, CONVERT(INT, SESSION_CONTEXT(N'UserId')));

-- Filter out the documents where the user signature is irrelevant
	INSERT INTO @FilteredEntities([Id])
	EXEC [bll].[Documents_Filter__Sign]
		@Entities = @Entities;
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Documents__Sign]
		@Entities = @FilteredEntities,
		@State = @State,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@AgentId = @AgentId,
		@RoleId = @RoleId,
		@SignedAt = @SignedAt;

	---- get the documents who satsified all the requirements for state change

	INSERT INTO @ReadyEntities([Id])
	EXEC [bll].[Documents_Ready__Select]
		@Entities = @FilteredEntities;
	
	IF EXISTS(SELECT * FROM @ReadyEntities)
		EXEC [dal].[Documents_State__Update]
			@Entities = @ReadyEntities,
			@State = @State;

	--IF @ReturnEntities = 1
	--	SELECT * FROM @ReadyEntities;
END;